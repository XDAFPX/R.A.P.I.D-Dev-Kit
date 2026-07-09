using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;
using Microsoft.Extensions.Logging;
using UnityEngine.Scripting;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    [LogLevel(LogLevel.Trace)]
    [Preserve]
    public struct OnObjectCreatedAsyncEvent : IObjectEvent
    {
        public OnObjectCreatedAsyncEvent(object o)
        {
            Object = o;
        }

        public object Object { get; }


        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(Adam), $"Object ({Object}) async created event fired");
        }
    }
}