using System;
using Bdeshi.Helpers.Utility;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DAFP.TOOLS.Common
{
    public interface IExpirable
    {
        public float LifeTime { get; set; }
        public ref FiniteTimer Timer { get; }
        public bool DoDestroyOnExpire { get; }
        public event Action EOnExpire;
        public void Expire(Object self)
        {
            if(DoDestroyOnExpire)
                Object.Destroy(self);
        }

        public void UpdateTimer(float delta,Object self)
        {
            if(Timer.tryCompleteTimer(delta))
                Expire(self);
        }
    }
}