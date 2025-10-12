using System.Collections.Generic;
using DAFP.TOOLS.ECS.ViewModel;
using RapidLib.DAFP.TOOLS.Common;
using UnityEditor.PackageManager.Requests;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class EmptyView : EntityPetImpl, IViewModel
    {

        public string Name { get; set; } = "Empty";
        public void Enable()
        {
        }

        public void Disable()
        {
        }

        public bool Enabled { get; } = false;
        public IViewModel Construct(IEntity owner)
        {
            ChangeOwner(owner);
            return this;
        }
    }
}