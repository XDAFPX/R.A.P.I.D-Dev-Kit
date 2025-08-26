using System;

namespace DAFP.TOOLS.Common.Utill
{
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = false)]
    public class BindName : Attribute
    {
        public string Name { get; }
        public BindName(string name) => Name = name;
    }
}