using System.Collections.Generic;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.Services;
using MessagePipe;
using Unity.GraphToolkit.Editor;
using UnityEngine;
using UnityEngine.Events;
using Zenject;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    public abstract class HurtBox<T> : EmptyEntity, IPetOf<HurtGroup<T>, HurtBox<T>>
    {
        public abstract HurtBoxData<T> GetCtx();
        [Inject] private IPublisher<OnHurtBoxFlaggedEvent> flaggedEvent;
        [Inject] private IPublisher<OnHurtBoxActivatedEvent> activatedEvent;
        [SerializeField] private UnityEvent OnHurt;
        [SerializeField] private UnityEvent<T> OnHurtT;

        protected override Bounds CalculateBounds()
        {
            var bb = GameUtils.CalculateCombinedBounds(this);
            bb.center = transform.position;
            return bb;
        }

        public void FlagAsHit()
        {
            flaggedEvent.Publish(new OnHurtBoxFlaggedEvent(this));
        }

        public void Hurt(HitBox<T> box, T objThatHurtYou)
        {
            OnHurt?.Invoke();
            OnHurtT?.Invoke(objThatHurtYou);

            activatedEvent.
                Publish(new OnHurtBoxActivatedEvent(this,HurtGroup,box,objThatHurtYou));
        }


        //------------------------------------ STUFFF

        public HurtGroup<T> HurtGroup => ((IPetOf<HurtGroup<T>, HurtBox<T>>)this).GetCurrentOwner();
        
        public List<HurtGroup<T>> Owners { get; } = new List<HurtGroup<T>>();
    }
}