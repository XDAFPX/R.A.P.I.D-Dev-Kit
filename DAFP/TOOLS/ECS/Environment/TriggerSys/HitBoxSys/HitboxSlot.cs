using System.Collections;
using DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    [System.Serializable]
    public struct HitboxSlot<T> : INameable
    {
        [SerializeField]private HitBox<T> HitBox;

        public HitboxSlot(string name)
        {
            Name = name;
            HitBox = null;
        }

        [field : SerializeField]public string Name { get; set; }

        public bool TryGet(out HitBox<T> hitBox)
        {
            hitBox = null;
            if (HitBox == null)
                return false;
            hitBox = HitBox;
            return true;
        }
        public HitBox<T> Get()
        {
#if UNITY_EDITOR 
            Debug.Assert(HitBox==null,$"HitBox slot of ({Name}) empty or unassigned!");
#endif
            return HitBox;
        }

        public static explicit operator HitBox<T>(HitboxSlot<T> slot)
        {
            return slot.Get();
        }
    }
}