using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using ITnnovative.AOP.Attributes.Method;
using ITnnovative.AOP.Processing.Execution.Arguments;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.Aspects
{
    public class EntNotifyAspect : Attribute,IMethodEnterAspect
    {
        public void OnMethodEnter(MethodExecutionArguments args)
        {
            // Debug.Log("adad");
            if (args.source is not IEntity _ent)
                return;
        
            var _method = args.rootMethod.Name;
            switch (_method)
            {
                case nameof(ICommonEntityInterface.IDieable.Die):
                    if (_ent is ICommonEntityInterface.IDieable _dieable)
                    {
                        process_entity_death(args, _ent, _dieable);
                    }
                    break;
                case nameof(IDamageable.TakeDamage):
                    if (_ent is IDamageable _dmg)
                    {
                        process_damage_event(args, _ent, _dmg);
                    }
                    break;
            }
        }
        
        private static void process_entity_death(MethodExecutionArguments args, IEntity ent, ICommonEntityInterface.IDieable dieable)
        {
            var _val = args.arguments[0].value as IDamage;
            var _event = new ICommonEntityEvent.EntityDieEvent(ent, _val);
            InvokeEvent(dieable, nameof(ICommonEntityInterface.IDieable.OnDie), dieable, _event);
            ent.BroadcastEvent(_event);
        }
        
        private static void process_damage_event(MethodExecutionArguments args, IEntity ent, IDamageable dmg)
        {
            var _val = args.arguments[0].value as IDamage;
            var _event = new ICommonEntityEvent.EntityTakeDamageEvent(ent, _val);
            InvokeEvent(dmg, nameof(IDamageable.OnTakeDamage),  _event);
            ent.BroadcastEvent(_event);
        }

        // Invokes an event on an object via reflection
        /// </summary>
        /// <param name="target">The object containing the event</param>
        /// <param name="eventName">The name of the event</param>
        /// <param name="args">The arguments to pass to the event handlers</param>
        /// <returns>True if the event was found and invoked, false otherwise</returns>
        public static bool InvokeEvent(object target, string eventName, params object[] args)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (string.IsNullOrEmpty(eventName))
                throw new ArgumentNullException(nameof(eventName));

            Type _type = target.GetType();

            // Try to find the backing field for the event
            // First try the event name directly
            FieldInfo _eventField = _type.GetField(eventName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            // If not found, try the compiler-generated name pattern
            if (_eventField == null)
            {
                _eventField = _type.GetField($"<{eventName}>k__BackingField",
                    BindingFlags.Instance | BindingFlags.NonPublic);
            }

            // If still not found, search all fields for a match
            if (_eventField == null)
            {
                var _fields = _type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var _field in _fields)
                {
                    if (_field.Name.Contains(eventName))
                    {
                        _eventField = _field;
                        break;
                    }
                }
            }

            if (_eventField == null)
                return false;

            // Get the delegate
            Delegate _eventDelegate = (Delegate)_eventField.GetValue(target);

            if (_eventDelegate == null)
                return false; // No subscribers

            // Invoke the event with the provided arguments
            _eventDelegate.DynamicInvoke(args);
            return true;
        }
    }
}