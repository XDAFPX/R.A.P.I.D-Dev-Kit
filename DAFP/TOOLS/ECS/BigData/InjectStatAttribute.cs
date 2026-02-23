using System;

namespace DAFP.TOOLS.ECS.BigData
{
    // Injection-only attribute. Used to fetch an already-declared stat into a field.
    // Ignored by FixStats(); handled by InjectStats().
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class InjectStatAttribute : Attribute
    {
        public string StatName { get; }

        public InjectStatAttribute(string statName)
        {
            StatName = statName;
        }
    }
}