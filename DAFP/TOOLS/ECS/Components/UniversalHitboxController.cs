using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using RapidLib.DAFP.TOOLS.Common;
using TNRD;
using UnityEngine;
using UnityEventBus;

namespace DAFP.TOOLS.ECS.Components
{
    public class UniversalHitboxControllerOfEntity : UniversalHitboxController<IEntity>
    {
    }

    public class UniversalHitboxController<T> : EntityComponent, IOwnerBase, IAct
    {
        public void Act()
        {
            foreach (var _realHitBox in Boxes.RealHitBoxes())
            {
                _realHitBox.Act();
            }
        }

        
        public HitBox<T> GetBox(string name) => Boxes.FindByName<HitboxSlot<T>>(name).Get();
        
        

        [SerializeField] protected List<HitboxSlot<T>> Boxes;

        
        
        protected override void OnTick()
        {
        }

        protected override void OnInitialize()
        {
            foreach (var _hitboxSlot in Boxes)
            {
                if (_hitboxSlot.TryGet(out var _box))
                {
                    ((IOwnedBy<IEntity>)_box).ChangeOwner(Host);
                }
            }
        }

        protected override void OnStart()
        {
        }

        public IEnumerable<object> AbsolutePets => Boxes.Select((slot => slot.Get()));

        private void Reset()
        {
            if (TryGetComponent(out ICommonEntityInterface.IHitBoxDefinition<T> _def))
            {
                Boxes = _def.HitBoxDefinition.ToList();
            }
        }
    }
}