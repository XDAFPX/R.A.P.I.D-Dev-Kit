using System.Collections.Generic;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.ViewModel;
using PixelRouge.CsharpExtensionMethods;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class RendererView : List<Renderer>, IViewModel
    {
        
        private EntityPetImpl Impl = new EntityPetImpl();

        public IEntity GetCurrentOwner()
        {
            return Impl.GetCurrentOwner();
        }

        public void ChangeOwner(IEntity newOwner)
        {
            Impl.ChangeOwner(newOwner);
        }

        public List<IEntity> Owners => Impl.Owners;

        public IEntity GetExOwner()
        {
            return Impl.GetExOwner();
        }

        public string Name { get; set; } = "Default";

        public void Enable()
        {
            this.ForEach((renderer => renderer.enabled = true));
            Enabled = true;
        }

        public void Disable()
        {
            this.ForEach((renderer => renderer.enabled = false));

            Enabled = false;
        }

        public bool Enabled { private set; get; } = true;
        public IViewModel Construct(IEntity owner)
        {
            Clear();
            
            Impl = new EntityPetImpl();
            ChangeOwner(owner);
            owner.GetWorldRepresentation().transform.GetComponentsInRoot<Renderer>().ForEachComponent((Add));
            return this;
        }
    }
}