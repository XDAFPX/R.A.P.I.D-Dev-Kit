using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using Microsoft.Extensions.Logging;

namespace DAFP.TOOLS.ECS.Basic.Events
{   
    [LogLevel(LogLevel.Trace)]
    public struct OnObjectRegisterEvent : IObjectEvent
    {
        public OnObjectRegisterEvent(object o)
        {
            Object = o;
        }

        public object Object { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(Adam), $"Object ({Object}) has been registered");
        }
    }
}