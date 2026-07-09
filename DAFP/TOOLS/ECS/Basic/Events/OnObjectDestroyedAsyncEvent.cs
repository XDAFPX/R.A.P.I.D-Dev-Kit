using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnObjectDestroyedAsyncEvent : IObjectEvent
    {
        public OnObjectDestroyedAsyncEvent(object o)
        {
            Object = o;
        }

        public object Object { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(Adam), $"Object ({Object}) async destroyed event fired");
        }
    }
}