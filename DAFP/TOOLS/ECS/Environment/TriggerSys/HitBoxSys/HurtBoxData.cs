using UnityEngine.Events;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    [System.Serializable]
    public struct HurtBoxData<T>
    {
        public T Ctx;
        public HurtBoxContext<T> Data;

        public HurtBoxData(T ctx,   HurtBoxContext<T> data)
        {
            Ctx = ctx;
            Data = data;
        }
    }
}