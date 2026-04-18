using System.Collections.Generic;
using DAFP.GAME.Assets;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BuiltIn;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys
{
    public abstract class HurtBox<T> : EmptyEntity, IPetOf<HurtGroup<T>, HurtBox<T>>
    {
        public abstract HurtBoxData<T> GetCtx();
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
            BroadcastEvent(new HurtBoxFlaggedNotActivatedEvent() { HurtBox = this });
        }

        public void Hurt(HitBox<T> box, T objThatHurtYou)
        {
            OnHurt?.Invoke();
            OnHurtT?.Invoke(objThatHurtYou);


            BroadcastEvent(new HurtBoxActivatedEvent() { HitBox = box, HurtBox = this, Obj = objThatHurtYou });
            BroadcastEvent(new HurtBoxActivatedEvent<T>()
            {
                Group = ((IPetOf<HurtGroup<T>, HurtBox<T>>)this).GetCurrentOwner(), HitBox = box, HurtBox = this,
                Obj = objThatHurtYou
            });
        }


        //------------------------------------ STUFFF
        public static THurtBox Construct<THurtBox>(GameObject obj, T owner, HurtGroup<T> hurtGroup,
            IAssetFactory factory)
            where THurtBox : HurtBox<T>
        {
            var _hurtBox = obj.AddEntity<THurtBox>(factory);
            if (_hurtBox is IOwnedBy<T> _pet)
            {
                _pet.ChangeOwner(owner);
            }

            ((IPetOf<HurtGroup<T>, HurtBox<T>>)_hurtBox).ChangeOwner(hurtGroup);

            return _hurtBox;
        }

        public List<HurtGroup<T>> Owners { get; } = new List<HurtGroup<T>>();
    }

    public struct HurtBoxFlaggedNotActivatedEvent
    {
        public IEntity HurtBox;
    }

    public struct HurtBoxActivatedEvent<T>
    {
        public HitBox<T> HitBox;
        public HurtBox<T> HurtBox;
        public HurtGroup<T> Group;
        public T Obj;
    }

    public struct HurtBoxActivatedEvent
    {
        public IEntity HitBox;
        public IEntity HurtBox;
        public object Obj;
    }
}