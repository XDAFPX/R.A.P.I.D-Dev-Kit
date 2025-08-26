using System;
using System.Collections.Generic;
using System.Reflection;
using DAFP.TOOLS.ECS.BuiltIn;
using UnityEngine.InputSystem;

namespace DAFP.TOOLS.Common.Utill
{
  public static class BindNameInitializer
    {
        public static void Initialize(IGamePlayer player)
        {
            // 1) reflect plugin type and its Binds dictionary
            var pluginType = player.GetType();

            // 2) gather methods and expected delegate type
            var allMethods   = MethodGetter.GetAllMethods(pluginType);
            var callbackType = typeof(InputAction.CallbackContext);
            var actionType   = typeof(Action<>).MakeGenericType(callbackType);

            foreach (var method in allMethods)
            {
                // 3) filter by [BindName]
                if (!Attribute.IsDefined(method, typeof(BindName)))
                    continue;

                var att   = method.GetCustomAttribute<BindName>();
                var parms = method.GetParameters();

                // 4) signature must be void Method(InputAction.CallbackContext)
                if (method.ReturnType != typeof(void) ||
                    parms.Length != 1 ||
                    parms[0].ParameterType != callbackType)
                    continue;

                // 5) create delegate for static or instance method
                Delegate del = method.IsStatic
                    ? Delegate.CreateDelegate(actionType, method)
                    : Delegate.CreateDelegate(actionType, player, method);

                // 6) add to plugin.Binds
                player.Binds.Add(att.Name, (Action<InputAction.CallbackContext>)del);
            }
        }
    }
}