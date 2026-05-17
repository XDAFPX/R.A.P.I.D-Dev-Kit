using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using UGizmo;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalDebugSystem<TMessenger, TGizmos> : IDebugSys<TGizmos, TMessenger>, ITickable where TGizmos :
        IGlobalGizmos
        where TMessenger :
        IConsoleMessenger
    {
        private List<IDebugDrawable> debugDrawers = new List<IDebugDrawable>();
        private List<IDebugSubSys> subSystems = new();

        [Inject]
        public UniversalDebugSystem(TMessenger messenger, TGizmos gizmos, IList<DebugDrawLayer> layers)
        {
            Messenger = messenger;
            if (Messenger is IOwnedBy<IDebugSys<IGlobalGizmos, IConsoleMessenger>> ownable)
                ownable.ChangeOwner((IDebugSys<IGlobalGizmos, IConsoleMessenger>)this);
            Gizmos = gizmos;
            Layers = layers;
        }

        public TGizmos Gizmos { get; }
        public TMessenger Messenger { get; }
        public IList<DebugDrawLayer> Layers { get; }
        public DebugDrawLayer GetSharedLayer => new DebugDrawLayer("SHARED", Application.isEditor);
        protected List<DebugValue> DebugValues = new List<DebugValue>();


        IEnumerable<IDebugDrawable> IOwnerOf<IDebugDrawable>.Pets => debugDrawers;

        public void AddPet(IDebugDrawable pet)
        {
            GameUtils.AddPet(pet, ref debugDrawers);
        }

        public bool RemovePet(IDebugDrawable pet)
        {
            return GameUtils.RemovePet(pet, ref debugDrawers);
        }

        public void AddPet(IDebugSubSys pet)
        {
            if (pet == null) return;
            if (subSystems.Contains(pet)) return;
            subSystems.Add(pet);
        }

        public bool RemovePet(IDebugSubSys pet)
        {
            if (pet == null) return false;
            if (!subSystems.Contains(pet)) return false;
            subSystems.Remove(pet);
            return true;
        }

        IEnumerable<IDebugSubSys> IOwnerOf<IDebugSubSys>.Pets => subSystems;

        public IEnumerable<object> AbsolutePets => debugDrawers.Union<object>(subSystems);

        public void AddDebugValue(DebugValue value)
        {
            if (!DebugValues.Contains(value))
                DebugValues.Add(value);

            Display.Value.UpdateValues(DebugValues);
        }

        public void RemoveDebugValue(string value)
        {
            var _found = DebugValues.FindByName<DebugValue>(value);
            if (!_found.Equals(default))
                DebugValues.Remove(_found);

            Display.Value.UpdateValues(DebugValues);
        }

        protected readonly Lazy<DebugValueDisplay> Display =
            new Lazy<DebugValueDisplay>(() => new DebugValueDisplay(Array.Empty<DebugValue>(), Color.white));

        public void Tick()
        {
            Messenger.Tick();
            ((IDrawable)this).Draw();
            Display.Value.Tick();
        }

        public void Draw()
        {
            foreach (var _ownable in ((IOwnerOf<IDebugDrawable>)this).Pets)
                _ownable.Draw();
        }
    }
}