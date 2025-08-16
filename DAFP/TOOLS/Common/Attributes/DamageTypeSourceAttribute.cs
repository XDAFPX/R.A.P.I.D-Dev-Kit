using System;

namespace DAFP.TOOLS.Common.Attributes
{
    /// <summary>
    /// Place this on your assembly to feed a JSON into the generator.
    /// Path is relative to the project root (or to an AdditionalFile entry).
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class DamageTypeSourceAttribute : Attribute
    {
        public string JsonPath { get; }
        public DamageTypeSourceAttribute(string jsonPath) => JsonPath = jsonPath;
    }
}
