using System;
using Microsoft.Extensions.Logging;

namespace DAFP.TOOLS.ECS.Basic
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class LogLevelAttribute : Attribute
    {
        public LogLevel Level { get; }
        public LogLevelAttribute(LogLevel level) => Level = level;
    }
}