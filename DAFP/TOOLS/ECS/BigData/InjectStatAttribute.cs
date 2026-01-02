using System;
using System.Collections.Generic;

namespace DAFP.TOOLS.ECS.BigData
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class InjectStatAttribute : Attribute
    {
        public string StatName { get; }
        public string FallbackFieldName { get; }
        public object FallbackObject { get; }

        public InjectStatAttribute(string statName, object fallbackObject )
        {
            StatName = statName;
            FallbackObject = fallbackObject;
        }

        public InjectStatAttribute(string statName, string fallbackFieldName = null)
        {
            StatName = statName;
            FallbackFieldName = fallbackFieldName;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class RequireStatAttribute : Attribute
    {
        // Marker attribute - presence means required
    }

// Exception

// Stat Injector
}