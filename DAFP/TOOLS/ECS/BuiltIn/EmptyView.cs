using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using DAFP.TOOLS.ECS.ViewModel;
using RapidLib.DAFP.TOOLS.Common;
using UnityEditor.PackageManager.Requests;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class EmptyView : IViewModel
    {
        public string Name { get; set; } = "Empty";

        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public bool Enabled { get; } = false;

        public IViewModel InitOwner(IEntity owner)
        {
            ((IPetOf<IEntity,IViewModel>)this).ChangeOwner(owner);
            return this;
        }

        public HurtGroup<IEntity> GetHurtGroup(IEntity owner)
        {
            return null;
        }

        public List<IEntity> Owners { get; } = new();
    }
}