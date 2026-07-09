using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using RapidLib.DAFP.TOOLS.Common;
using UGizmo;

namespace DAFP.TOOLS.ECS.Thinkers
{
    public class DummyThinker : IThinker, IThinkerLogic
    {
        private List<IDebugDrawable> owners = new();
        private List<IThinker> owners1 = new();
        private List<IDebugDrawable> debugDrwables = new();

        List<IThinker> IPetOwnerTreeOf<IThinker>.Owners => owners1;
        List<IDebugDrawable> IPetOf<IDebugDrawable, IDebugDrawable>.Owners => owners;

        public List<IThinker> Children { get; } = new();
        public IEnumerable<IDebugDrawable> Pets => debugDrwables;

        public bool HasWoken { get; set; }

        public IDebugSys<IGlobalGizmos, IConsoleMessenger> DebugSystem { get; }
        public string Name { get; set; } = nameof(DummyThinker);

        public void AddPet(IDebugDrawable pet)
        {
            GameUtils.AddPet(pet, debugDrwables);
        }

        public bool RemovePet(IDebugDrawable pet)
        {
            return GameUtils.RemovePet(pet, debugDrwables);
        }


        void IThinkerLogic.Start(IEntity host)
        {
            foreach (var _thinker in Children)
            {
                if (_thinker is IThinkerLogic _logic)
                    _logic.Start(host);
            }
        }

        void IThinkerLogic.Tick(IEntity host, ITickerBase ticker)
        {
            foreach (var _thinker in Children)
            {
                if (_thinker is IThinkerLogic _logic)
                    _logic.Tick(host, ticker);
            }
        }


        void IThinkerLogic.End(IEntity host)
        {
            foreach (var _thinker in Children)
            {
                if (_thinker is IThinkerLogic _logic)
                    _logic.End(host);
            }
        }



        public IEnumerable<object> AbsolutePets => Children;
    }
}