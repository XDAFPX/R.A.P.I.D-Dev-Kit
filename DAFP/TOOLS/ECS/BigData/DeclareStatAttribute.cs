using System;
using System.IO;
using System.Linq;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class DeclareStatAttribute : Attribute, INameable
    {
        public string StatName { get; }
        public object FallbackObject { get; }

        public DeclareStatAttribute(string statName, object fallbackObject)
        {
            StatName = statName;
            FallbackObject = fallbackObject;
        }

        public uint DephRating()
        {
            return (uint)StatName.Count(c => c == '/');
        }

        public string Leaf => Path.GetFileNameWithoutExtension(StatName);
        public string Name
        {
            get => StatName;
            set { }
        }
    }
}