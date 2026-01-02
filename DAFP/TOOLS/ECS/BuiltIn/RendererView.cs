using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.ViewModel;
using PixelRouge.CsharpExtensionMethods;
using RapidLib.DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class RendererView : List<Renderer>, IViewModel
    {
        public string Name { get; set; } = "Default";

        public void Enable()
        {
            ForEach(renderer => renderer.enabled = true);
            Enabled = true;
        }

        public void Disable()
        {
            ForEach(renderer => renderer.enabled = false);

            Enabled = false;
        }

        public bool Enabled { private set; get; } = true;

        public IViewModel InitOwner(IEntity owner)
        {
            Clear();

            ((IEntityPet)this).ChangeOwner(owner);
            owner.GetWorldRepresentation().transform.GetComponentsInRoot<Renderer>().ForEachComponent(Add);
            return this;
        }

        public List<IEntity> Owners { get; } = new();
    }
}