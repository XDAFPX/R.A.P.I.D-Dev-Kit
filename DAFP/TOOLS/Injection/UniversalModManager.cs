using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using RapidLib.DAFP.TOOLS.Common;
using TripleA.Utils.Extensions;
using UGizmo;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.Injection
{
    public class UniversalModManager : IModManager,IResetable,INameable
    {
        private readonly IEventBus bus;
        private readonly IDebugSys<IGlobalGizmos, IConsoleMessenger> debug;
        private readonly DiContainer injector;

        [Inject]public UniversalModManager([Inject(Id = "GlobalGameEventsBus")] IEventBus bus,IDebugSys<IGlobalGizmos,IConsoleMessenger> debug, DiContainer injector, IMod[] initialMods)
        {
            this.bus = bus;
            this.debug = debug;
            this.injector = injector;
            
            
            initialMods.ForEach((RegisterMod));
        }

        private List<IMod> installed = new();
        public void RegisterMod(IMod mod)
        {
            debug.Log(this, $"Registered mod : '{mod.Name}' from {mod.Author} ");
            bus.Subscribe(mod);
            injector.Inject(mod);
            installed.Add(mod);
        }

        public void UnRegisterMod(IMod mod)
        {
            bus.UnSubscribe(mod);
            mod.Dispose();
            installed.Remove(mod);
        }

        public IEnumerable<IMod> Mods()
        {
            return installed;
        }

        public void ResetToDefault()
        {
            var _l = installed.Clone();
            foreach (var _mod in _l)
            {
                UnRegisterMod(_mod);
            }
        }

        public string Name { get; set; } = nameof(IModManager);
    }

    public interface IModManager
    {
        public void RegisterMod(IMod mod);
        public void UnRegisterMod(IMod mod);
        public IEnumerable<IMod> Mods();
    }

    public interface IMod : ISubscriber, IDisposable,INameable,IDescriptable, IAuthorContainer
    {
    }

}