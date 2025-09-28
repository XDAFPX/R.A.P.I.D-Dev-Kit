using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using DAFP.TOOLS.ECS.BuiltIn;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.Common.Utill
{
    public static class BindNameInitializer
    {
        public static void Initialize(IBindedEntity player)
        {
            // 1) reflect plugin type and its Binds dictionary
            var pluginType = player.GetType();

            // 2) gather methods and expected delegate type
            var allMethods   = MethodGetter.GetAllMethods(pluginType);
            var callbackType = typeof(InputBind.CallbackContext);

            foreach (var method in allMethods)
            {
                // 3) filter by [BindName]
                if (!Attribute.IsDefined(method, typeof(BindName)))
                    continue;

                var att   = method.GetCustomAttribute<BindName>();
                var parms = method.GetParameters();

                // 4) only accept signature void or Task Method(InputBind.CallbackContext)
                if (parms.Length != 1 || parms[0].ParameterType != callbackType)
                    continue;

                bool returnsVoid = method.ReturnType == typeof(void);
                bool returnsTask = method.ReturnType == typeof(Task);
                if (!returnsVoid && !returnsTask)
                    continue;

                Action<InputBind.CallbackContext> wrapper;

                if (returnsVoid)
                {
                    // 5a) for void methods, directly create Action<CallbackContext>
                    var del = method.IsStatic
                        ? Delegate.CreateDelegate(typeof(Action<InputBind.CallbackContext>), method)
                        : Delegate.CreateDelegate(typeof(Action<InputBind.CallbackContext>), player, method);

                    wrapper = (Action<InputBind.CallbackContext>)del;
                }
                else
                {
                    // 5b) for async Task methods, create Func<CallbackContext, Task> then wrap
                    var funcType = typeof(Func<,>).MakeGenericType(callbackType, typeof(Task));
                    var funcDel = method.IsStatic
                        ? Delegate.CreateDelegate(funcType, method)
                        : Delegate.CreateDelegate(funcType, player, method);

                    var invokeAsync = (Func<InputBind.CallbackContext, Task>)funcDel;

                    wrapper = ctx =>
                    {
                        // fire-and-forget, optionally handle exceptions
                        _ = invokeAsync(ctx).ContinueWith(t =>
                        {
                            Debug.LogException(t.Exception);
                        }, TaskContinuationOptions.OnlyOnFaulted);
                    };
                }

                player.Binds.Add(att.Name, wrapper);
            }
        }
    }
}
