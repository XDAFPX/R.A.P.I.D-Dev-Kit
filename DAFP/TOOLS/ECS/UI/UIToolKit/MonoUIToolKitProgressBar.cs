using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace DAFP.TOOLS.ECS.UIToolKit
{
    public class MonoUIToolKitProgressBar : MonoUIToolKitElement<ProgressBar>
    {
        [SerializeField] private string HideClass;


        protected override Dictionary<string, (Func<object>, Action<object>)> GetInitialAttributes()
        {
            return new Dictionary<string, (Func<object>, Action<object>)>
            {
                {
                    "Value",
                    (
                        () => Element.value,
                        o => Element.value = Convert.ToSingle(o)
                    )
                },
                {
                    "MaxValue",
                    (
                        () => Element.highValue,
                        o => Element.highValue = Convert.ToSingle(o)
                    )
                },
                {
                    "Text",
                    (
                        () => Element.title,
                        o => Element.title = (string)o
                    )
                }
            };
        }


        protected override bool CurrentlyVisible()
        {
            return Element.ClassListContains(HideClass);
        }

        public override void Show()
        {
            Element.RemoveFromClassList(HideClass);
        }

        public override void Hide()
        {
            Element.AddToClassList(HideClass);
        }

        public override void SubscribeToCallback(string name, Action<InputBind.CallbackContext> action)
        {
        }

        public override void UnSubscribeToCallback(string name, Action<InputBind.CallbackContext> action)
        {
        }

        public override void UnSubscribeFrom(string name)
        {
        }

        public override void UnSubscribeAll()
        {
        }
    }
}