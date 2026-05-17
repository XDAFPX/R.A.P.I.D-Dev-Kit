using System.Collections.Generic;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface IHitBoxDefinition<T>
    {
        IEnumerable<HitboxSlot<T>> HitBoxDefinition { get; }
    }
}
