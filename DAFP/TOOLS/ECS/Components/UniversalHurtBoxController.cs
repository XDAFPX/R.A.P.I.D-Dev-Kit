using System.Collections.Generic;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Components
{
    public class UniversalHurtBoxControllerOfEntity : UniversalHurtBoxController<IEntity>{}
    public class UniversalHurtBoxController<T> : EntityComponent, IHurtBoxController<T>
    {
        [ReadOnly(OnlyWhilePlaying = true)][SerializeField] private List<HurtGroup<T>> Groups = new List<HurtGroup<T>>();

        protected override void OnTick()
        {
        }

        protected override void OnInitialize()
        {
            foreach (var _hurtGroup in Groups)
            {
                ((IOwnedBy<IHurtBoxController<T>>)_hurtGroup).ChangeOwner(this);
                foreach (var _box in _hurtGroup.Pets)
                {
                    ((IOwnedBy<IEntity>)_box).ChangeOwner(Host);
                }
            }
        }

        protected override void OnStart()
        {
        }

        public IEnumerable<HurtGroup<T>> Pets => Groups;

        public void AddPet(HurtGroup<T> pet)
        {
            GameUtils.AddPet(pet, ref Groups);
        }

        public bool RemovePet(HurtGroup<T> pet)
        {
            return GameUtils.RemovePet(pet, ref Groups);
        }
        public IEntity GetCurrentOwner()
        {
            return Host;
        }

        public void ChangeOwner(IEntity newOwner)
        {
            return;
        }
    }
}