using DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    public class EntityHurtBox : HurtBox<IEntity> , IOwnedBy<IEntity>
    {
        public Entity Reference;
        public override HurtBoxData<IEntity> GetCtx()
        {
            var ownable = ((IOwnedBy<IEntity>)this);
            var ent = ownable.GetCurrentOwner() != null ? ownable.GetCurrentOwner() : Reference;
            #if UNITY_EDITOR
            Debug.Assert(ent!=null,"HurtBoxOfEntity without it's owner has been hit. (ent!=null) ");
            #endif
            return new HurtBoxData<IEntity>(ent,  new HurtBoxContext<IEntity>(this,((IOwnedBy<HurtGroup<IEntity>>)this).GetCurrentOwner()) );
        }
    }
}