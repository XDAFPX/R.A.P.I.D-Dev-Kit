using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using ITnnovative.AOP.Attributes;
using ITnnovative.AOP.Attributes.Event;
using ITnnovative.AOP.Attributes.Method;
using ITnnovative.AOP.Attributes.Property;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using MethodAttributes = Mono.Cecil.MethodAttributes;
using MethodBody = Mono.Cecil.Cil.MethodBody;
using ParameterAttributes = Mono.Cecil.ParameterAttributes;

namespace ITnnovative.AOP.Processing.Editor
{
    [InitializeOnLoad]
    public static class CodeProcessor
    {
        static CodeProcessor()
        {
            AssemblyReloadEvents.beforeAssemblyReload += WeaveEditorAssemblies;
        }

        /// <summary>
        /// Cache for Attribute Types
        /// </summary>
        private static Dictionary<Type, List<Type>> _typeCache = new Dictionary<Type, List<Type>>();

        /// <summary>
        /// Path to store assembly hash cache
        /// </summary>
        private static readonly string HashCachePath = Path.Combine(
            Application.dataPath, 
            "../Library/AOPWeavingCache.json"
        );

        public const bool DEBUG = false;

        /// <summary>
        /// Container for storing assembly hash information
        /// </summary>
        [Serializable]
        private class AssemblyHashCache
        {
            public Dictionary<string, string> FileHashes = new Dictionary<string, string>();
        }

        /// <summary>
        /// Calculate MD5 hash of a file
        /// </summary>
        private static string CalculateFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        /// <summary>
        /// Load the hash cache from disk
        /// </summary>
        private static AssemblyHashCache LoadHashCache()
        {
            if (!File.Exists(HashCachePath))
            {
                return new AssemblyHashCache();
            }

            try
            {
                var json = File.ReadAllText(HashCachePath);
                return JsonConvert.DeserializeObject<AssemblyHashCache>(json) ?? new AssemblyHashCache();
            }
            catch (Exception ex)
            {
                if (DEBUG)
                    Debug.LogWarning($"[Unity AOP] Failed to load hash cache: {ex.Message}");
                return new AssemblyHashCache();
            }
        }

        /// <summary>
        /// Save the hash cache to disk
        /// </summary>
        private static void SaveHashCache(AssemblyHashCache cache)
        {
            try
            {
                var json = JsonConvert.SerializeObject(cache, Formatting.Indented);
                File.WriteAllText(HashCachePath, json);
            }
            catch (Exception ex)
            {
                if (DEBUG)
                    Debug.LogWarning($"[Unity AOP] Failed to save hash cache: {ex.Message}");
            }
        }

        /// <summary>
        /// Filter DLL files to only those that have changed since last weaving
        /// </summary>
        private static string[] FilterChangedAssemblies(string[] dllFiles)
        {
            var cache = LoadHashCache();
            var changedFiles = new List<string>();

            foreach (var filePath in dllFiles)
            {
                // Skip if file doesn't exist
                if (!File.Exists(filePath))
                    continue;

                var currentHash = CalculateFileHash(filePath);
                
                // Check if hash exists and matches
                if (cache.FileHashes.TryGetValue(filePath, out var cachedHash))
                {
                    if (currentHash != cachedHash)
                    {
                        if (DEBUG)
                            Debug.Log($"[Unity AOP] Assembly changed: {Path.GetFileName(filePath)}");
                        changedFiles.Add(filePath);
                    }
                    else if (DEBUG)
                    {
                        Debug.Log($"[Unity AOP] Assembly unchanged, skipping: {Path.GetFileName(filePath)}");
                    }
                }
                else
                {
                    // New file or first run
                    if (DEBUG)
                        Debug.Log($"[Unity AOP] New assembly detected: {Path.GetFileName(filePath)}");
                    changedFiles.Add(filePath);
                }
            }

            return changedFiles.ToArray();
        }

        /// <summary>
        /// Update hash cache with newly weaved assemblies
        /// </summary>
        private static void UpdateHashCache(string[] weavedFiles)
        {
            var cache = LoadHashCache();

            foreach (var filePath in weavedFiles)
            {
                if (File.Exists(filePath))
                {
                    var hash = CalculateFileHash(filePath);
                    cache.FileHashes[filePath] = hash;
                }
            }

            SaveHashCache(cache);
        }

        public static void WeaveAssembly(AssemblyDefinition assembly)
        {
            // For all types in every module
            foreach (var module in assembly.Modules)
            {
                foreach (var type in module.Types)
                {
                    // For all constructors
                    for (var index = 0; index < type.Methods.Count; index++)
                    {
                        var method = type.Methods[index];
                        if (!HasAttributeOfType<AOPGeneratedAttribute>(method))
                        {
                            // Register AOP Processor by encapsulating method
                            if (HasAttributeOfType<IMethodAspect>(method))
                            {
                                MarkAsProcessed(module, method);
                                EncapsulateMethod(assembly, module, type, method);
                            }
                        }
                    }

                    for (var index = 0; index < type.Events.Count; index++)
                    {
                        var evt = type.Events[index];
                        if (!HasAttributeOfType<AOPGeneratedAttribute>(evt))
                        {
                            // Register AOP Processor by encapsulating method
                            if (HasAttributeOfType<IEventAspect>(evt))
                            {
                                MarkAsProcessed(module, evt);
                                if (HasAttributeOfType<IEventAddedListenerAspect>(evt))
                                {
                                    EncapsulateMethod(assembly, module, type, evt.AddMethod, evt.Name,
                                        nameof(AOPProcessor.OnEventListenerAdded));
                                }

                                if (HasAttributeOfType<IEventRemovedListenerAspect>(evt))
                                {
                                    EncapsulateMethod(assembly, module, type, evt.RemoveMethod, evt.Name,
                                        nameof(AOPProcessor.OnEventListenerRemoved));
                                }

                                if (HasAttributeOfType<IEventInvokedAspect>(evt))
                                {
                                    EncapsulateEventExecution(module, type, evt);
                                }
                            }
                        }
                    }

                    for (var index = 0; index < type.Properties.Count; index++)
                    {
                        var property = type.Properties[index];
                        if (!HasAttributeOfType<AOPGeneratedAttribute>(property))
                        {
                            // Register AOP Processor by encapsulating method
                            if (HasAttributeOfType<IPropertyAspect>(property))
                            {
                                MarkAsProcessed(module, property);
                                if (HasAttributeOfType<IPropertyGetAspect>(property))
                                {
                                    EncapsulateMethod(assembly, module, type, property.GetMethod, property.Name,
                                        nameof(AOPProcessor.OnPropertyGet));
                                }

                                if (HasAttributeOfType<IPropertySetAspect>(property))
                                {
                                    EncapsulateMethod(assembly, module, type, property.SetMethod, property.Name,
                                        nameof(AOPProcessor.OnPropertySet));
                                }
                            }
                        }
                    }
                }
            }

            // Check if we have valid symbols before trying to write them
            bool hasValidSymbols = assembly.MainModule.HasSymbols;

            try
            {
                var writerParameters = new WriterParameters { WriteSymbols = hasValidSymbols };
                assembly.Write(writerParameters);
            }
            catch (Exception ex) when (hasValidSymbols)
            {
                // If writing with symbols fails, retry without them
                if (DEBUG)
                    Debug.LogWarning($"[Unity AOP] Failed to write symbols, retrying without: {ex.Message}");

                var writerParameters = new WriterParameters { WriteSymbols = false };
                assembly.Write(writerParameters);
            }
            finally
            {
                assembly.Dispose();
            }
        }

        public static void EncapsulateEventExecution(ModuleDefinition module, TypeDefinition type, EventDefinition evt)
        {
            foreach (var method in type.Methods)
            {
                var body = method.Body;
                for (var pos = body.Instructions.Count - 1; pos >= 0; pos--)
                {
                    var instr = body.Instructions[pos];
                    if (instr.OpCode == OpCodes.Ldfld)
                    {
                        if (!HasAttributeOfType<IBeforeEventInvokedAspect>(evt)) continue;

                        if (instr.Operand is FieldDefinition)
                        {
                            var opObj = (FieldDefinition)instr.Operand;
                            if (method.Name.StartsWith("add_" + evt.Name) ||
                                method.Name.StartsWith("remove_" + evt.Name)) continue;

                            if (opObj.Name == evt.Name)
                            {
                                var newMethodBody = new List<Instruction>();
                                newMethodBody.Add(
                                    Instruction.Create(method.IsStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0));
                                newMethodBody.Add(Instruction.Create(OpCodes.Ldtoken, type));
                                newMethodBody.Add(Instruction.Create(OpCodes.Call, typeof(Type).GetMonoMethod(module,
                                    nameof(Type.GetTypeFromHandle))));
                                newMethodBody.Add(Instruction.Create(OpCodes.Ldstr, evt.Name));

                                if (module.HasType(typeof(AOPProcessor)))
                                {
                                    newMethodBody.Add(Instruction.Create(OpCodes.Call,
                                        module.GetType(typeof(AOPProcessor))
                                            .GetMethod(nameof(AOPProcessor.BeforeEventInvoked))));
                                }
                                else
                                {
                                    newMethodBody.Add(Instruction.Create(OpCodes.Call,
                                        typeof(AOPProcessor).GetMonoMethod(module,
                                            nameof(AOPProcessor.BeforeEventInvoked))));
                                }

                                var index = method.Body.Instructions.IndexOf(instr) - 1;
                                foreach (var i in newMethodBody)
                                {
                                    index++;
                                    method.Body.Instructions.Insert(index, i);
                                }
                            }
                        }
                    }
                    else if (instr.OpCode == OpCodes.Callvirt)
                    {
                        if (!HasAttributeOfType<IAfterEventInvokedAspect>(evt)) continue;

                        //evt.
                        var operand = instr.Operand as MethodReference;
                        if (operand == null)
                            throw new Exception("[Unity AOP] Unknown error, please report with source code.");

                        var opName = operand?.DeclaringType.FullName;
                        if (opName == evt.EventType.FullName)
                        {
                            var newMethodBody = new List<Instruction>();
                            newMethodBody.Add(Instruction.Create(method.IsStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0));
                            newMethodBody.Add(Instruction.Create(OpCodes.Ldtoken, type));
                            newMethodBody.Add(Instruction.Create(OpCodes.Call, typeof(Type).GetMonoMethod(module,
                                nameof(Type.GetTypeFromHandle))));
                            newMethodBody.Add(Instruction.Create(OpCodes.Ldstr, evt.Name));

                            if (module.HasType(typeof(AOPProcessor)))
                            {
                                newMethodBody.Add(Instruction.Create(OpCodes.Call,
                                    module.GetType(typeof(AOPProcessor))
                                        .GetMethod(nameof(AOPProcessor.AfterEventInvoked))));
                            }
                            else
                            {
                                newMethodBody.Add(Instruction.Create(OpCodes.Call, typeof(AOPProcessor).GetMonoMethod(
                                    module,
                                    nameof(AOPProcessor.AfterEventInvoked))));
                            }

                            var index = method.Body.Instructions.IndexOf(instr);
                            foreach (var i in newMethodBody)
                            {
                                index++;
                                method.Body.Instructions.Insert(index, i);
                            }
                        }
                    }
                }
            }
        }

        private static void EncapsulateMethod(AssemblyDefinition assembly, ModuleDefinition module,
            TypeDefinition type, MethodDefinition method, string overrideName = null, string overrideMethod =
                nameof(AOPProcessor.OnMethod))
        {
            // New body for current method (capsule)
            var newMethodBody = new List<Instruction>();
            newMethodBody.Add(
                Instruction.Create(method.IsStatic ? OpCodes.Ldnull : OpCodes.Ldarg_0)); // ldnull / ldarg_0
            newMethodBody.Add(Instruction.Create(OpCodes.Ldtoken, type));
            newMethodBody.Add(Instruction.Create(OpCodes.Call,
                typeof(Type).GetMonoMethod(module, nameof(Type.GetTypeFromHandle))));

            var mName = method.Name;
            if (!string.IsNullOrEmpty(overrideName))
                mName = overrideName;

            newMethodBody.Add(Instruction.Create(OpCodes.Ldstr, mName));
            newMethodBody.Add(Instruction.Create(OpCodes.Ldc_I4, method.Parameters.Count));
            newMethodBody.Add(Instruction.Create(OpCodes.Newarr, module.ImportReference(typeof(object))));

            for (var num = 0; num < method.Parameters.Count; num++)
            {
                var param = method.Parameters[num];
                var pType = param.ParameterType;

                newMethodBody.Add(Instruction.Create(OpCodes.Dup));
                newMethodBody.Add(Instruction.Create(OpCodes.Ldc_I4, num));
                newMethodBody.Add(Instruction.Create(OpCodes.Ldarg, param));
                if (param.ParameterType.IsValueType || param.ParameterType.IsGenericParameter)
                    newMethodBody.Add(Instruction.Create(OpCodes.Box, pType));
                newMethodBody.Add(Instruction.Create(OpCodes.Stelem_Ref));
            }


            if (module.HasType(typeof(AOPProcessor)))
            {
                newMethodBody.Add(Instruction.Create(OpCodes.Call,
                    module.GetType(typeof(AOPProcessor))
                        .GetMethod(overrideMethod)));
            }
            else
            {
                newMethodBody.Add(Instruction.Create(OpCodes.Call, typeof(AOPProcessor).GetMonoMethod(module,
                    overrideMethod)));
            }

            if (method.ReturnType.FullName != typeof(void).FullName)
            {
                if (method.ReturnType.IsValueType)
                {
                    newMethodBody.Add(Instruction.Create(OpCodes.Unbox_Any, method.ReturnType));
                }
            }
            else
            {
                newMethodBody.Add(Instruction.Create(OpCodes.Pop));
            }

            newMethodBody.Add(Instruction.Create(OpCodes.Ret));

            // Create new method
            var internalMethod =
                new MethodDefinition(method.Name + AOPProcessor.APPENDIX, method.Attributes, method.ReturnType);
            foreach (var param in method.Parameters)
            {
                var newParam = new ParameterDefinition(param.Name, param.Attributes, param.ParameterType);
                newParam.HasDefault = false;
                newParam.IsOptional = false;
                internalMethod.Parameters
                    .Add(newParam);
            }

            // Copy generic parameters
            foreach (var genericParameter in method.GenericParameters)
            {
                internalMethod.GenericParameters
                    .Add(new GenericParameter(genericParameter.Name, internalMethod));
            }

            var bodyClone = new MethodBody(method);
            bodyClone.AppendInstructions(newMethodBody);
            bodyClone.MaxStackSize = 8;

            // Replace method bodies
            internalMethod.Body = method.Body;
            method.Body = bodyClone;
            type.Methods.Add(internalMethod);
        }

        /// <summary>
        /// Marks member as processed by AOP
        /// </summary>
        public static void MarkAsProcessed(ModuleDefinition module, IMemberDefinition obj)
        {
            // For Assembly-CSharp load AOPGeneratedAttribute..ctor from local, otherwise reference ..ctor
            if (module.HasType(typeof(AOPGeneratedAttribute)))
            {
                var attribute = module.GetType(typeof(AOPGeneratedAttribute))
                    .GetConstructors().First();
                obj.CustomAttributes.Add(new CustomAttribute(attribute));
            }
            else
            {
                var attribute = module.ImportReference(
                    typeof(AOPGeneratedAttribute).GetConstructors((BindingFlags)int.MaxValue)
                        .First());
                obj.CustomAttributes.Add(new CustomAttribute(attribute));
            }
        }


        public static Instruction InsertInstructionAfter(this ILProcessor processor, Instruction afterThis,
            Instruction insertThis)
        {
            processor.InsertAfter(afterThis, insertThis);
            return insertThis;
        }

        /// <summary>
        /// Weave all assemblies available at specified path
        /// </summary>
        /// <param name="dllFiles">Files to weave. Must be *.dll</param>
        public static void WeaveAssembliesAtPaths(string[] dllFiles)
        {
            if (DEBUG)
                Debug.Log($"[Unity AOP] Weaving assemblies...");

            var successfullyWeavedFiles = new List<string>();

            foreach (var filePath in dllFiles)
            {
                if (filePath.Contains("Mono.Cecil.Rocks.dll") ||
                    filePath.Contains("Mono.Cecil.dll") ||
                    filePath.Contains("Newtonsoft.Json.dll") ||
                    filePath.Contains("UnityEngine") || 
                    filePath.Contains("Editor"))
                    continue;

                if (DEBUG)
                    Debug.Log($"[Unity AOP] Weaving {filePath}");

                // Setup assembly resolver
                var resolver = new DefaultAssemblyResolver();

                // Add Unity engine assemblies location
                string unityEnginePath = Path.GetDirectoryName(typeof(UnityEngine.Debug).Assembly.Location);
                resolver.AddSearchDirectory(unityEnginePath);

                // Add Unity editor assemblies location  
                string unityEditorPath = Path.GetDirectoryName(typeof(UnityEditor.Editor).Assembly.Location);
                resolver.AddSearchDirectory(unityEditorPath);

                // Add ScriptAssemblies directory
                string scriptAssembliesPath = Path.GetDirectoryName(filePath);
                resolver.AddSearchDirectory(scriptAssembliesPath);

                try
                {
                    var readerParameters = new ReaderParameters
                    {
                        ReadWrite = true,
                        AssemblyResolver = resolver,
                        ReadSymbols = false, // Changed to false - safer for Unity
                        ThrowIfSymbolsAreNotMatching = false
                    };

                    // Try to read with symbols first (for better debugging)
                    try
                    {
                        readerParameters.ReadSymbols = true;
                        var assembly = AssemblyDefinition.ReadAssembly(filePath, readerParameters);
                        WeaveAssembly(assembly);
                        successfullyWeavedFiles.Add(filePath);
                    }
                    catch (Exception)
                    {
                        // If reading with symbols fails, try without
                        readerParameters.ReadSymbols = false;
                        var assembly = AssemblyDefinition.ReadAssembly(filePath, readerParameters);
                        WeaveAssembly(assembly);
                        successfullyWeavedFiles.Add(filePath);
                    }
                }
                catch (Exception ex)
                {
                    if (DEBUG)
                        Debug.LogWarning(
                            $"[Unity AOP] Failed to weave assembly '{filePath}': {ex.Message}\n{ex.StackTrace}");
                }
                finally
                {
                    resolver?.Dispose();
                }
            }

            // Update hash cache for successfully weaved files
            if (successfullyWeavedFiles.Count > 0)
            {
                UpdateHashCache(successfullyWeavedFiles.ToArray());
            }
        }

        /// <summary>
        /// Get editor assemblies paths
        /// </summary>
        /// <returns></returns>
        public static string[] GetEditorAssembliesPaths()
        {
            var directoryPath = Application.dataPath + $"/../Library/ScriptAssemblies/";
            if (Directory.Exists(directoryPath))
            {
                var dllFiles = Directory.GetFiles(directoryPath, "*.dll");
                return dllFiles;
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Weave assemblies in editor - only reweaves changed assemblies
        /// </summary>
        public static void WeaveEditorAssemblies()
        {
            var allDllFiles = GetEditorAssembliesPaths();
            var changedDllFiles = FilterChangedAssemblies(allDllFiles);
            
            if (changedDllFiles.Length == 0)
            {
                if (DEBUG)
                    Debug.Log("[Unity AOP] No assemblies changed, skipping weaving.");
                return;
            }

            if (DEBUG)
                Debug.Log($"[Unity AOP] Found {changedDllFiles.Length} changed assemblies out of {allDllFiles.Length} total.");

            WeaveAssembliesAtPaths(changedDllFiles);
        }

        /// <summary>
        /// Force weave all assemblies, ignoring cache
        /// </summary>
        [MenuItem("Tools/AOP/Force Weave All Assemblies")]
        public static void ForceWeaveAllAssemblies()
        {
            Debug.Log("[Unity AOP] Force weaving all assemblies...");
            
            // Clear cache
            if (File.Exists(HashCachePath))
            {
                File.Delete(HashCachePath);
            }

            var dllFiles = GetEditorAssembliesPaths();
            WeaveAssembliesAtPaths(dllFiles);
            
            Debug.Log("[Unity AOP] Force weave completed.");
        }

        /// <summary>
        /// Clear the weaving cache
        /// </summary>
        [MenuItem("Tools/AOP/Clear Weaving Cache")]
        public static void ClearWeavingCache()
        {
            if (File.Exists(HashCachePath))
            {
                File.Delete(HashCachePath);
                Debug.Log("[Unity AOP] Weaving cache cleared.");
            }
            else
            {
                Debug.Log("[Unity AOP] No weaving cache found.");
            }
        }

        /// <summary>
        /// Check if member has attribute
        /// </summary>
        public static bool HasAttributeOfType<T>(IMemberDefinition member)
        {
            var subtypes = FindSubtypesOf<T>();
            foreach (var attribute in member.CustomAttributes)
            {
                foreach (var st in subtypes)
                {
                    if (attribute.AttributeType.FullName.Equals(st.FullName)) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Find subtypes of T, used for finding children attributes for aspects
        /// </summary>
        public static List<Type> FindSubtypesOf<T>()
        {
            var outObj = new List<Type>();

            // Check cache for result (improves efficiency)
            var mainType = typeof(T);
            if (_typeCache.ContainsKey(mainType))
                return _typeCache[mainType];

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in a.GetTypes())
                {
                    if (mainType.IsAssignableFrom(t))
                    {
                        outObj.Add(t);
                    }
                }
            }

            _typeCache[mainType] = outObj;

            return outObj;
        }
    }
}