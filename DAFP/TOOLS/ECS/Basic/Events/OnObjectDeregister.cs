using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using Microsoft.Extensions.Logging;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    [LogLevel(LogLevel.Trace)]
    public struct OnObjectDeregister : IObjectEvent
    {
        public OnObjectDeregister(object o)
        {
            Object = o;
        }

        public object Object { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(Adam), $"Object ({Object}) has been DEregistered");
        }
    }
}