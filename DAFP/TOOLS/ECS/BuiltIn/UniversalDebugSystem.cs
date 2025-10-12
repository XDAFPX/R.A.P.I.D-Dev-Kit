using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.ECS.DebugSystem;
using UGizmo;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalDebugSystem<TMessenger, TGizmos> : IDebugSys<TGizmos, TMessenger> where TGizmos :
        IGlobalGizmos
        where TMessenger :
        IMessenger
    {
        [Inject]
        public UniversalDebugSystem(TMessenger messenger, TGizmos gizmos, IList<DebugDrawLayer> layers)
        {
            Messenger = messenger;
            Gizmos = gizmos;
            Layers = layers;
        }

        public TGizmos Gizmos { get; } 
        public TMessenger Messenger { get; }
        public IList<DebugDrawLayer> Layers { get; }
        public DebugDrawLayer GetSharedLayer { get; } 


        private readonly HashSet<IOwnable<IDebugDrawable>> _pets = new();

        public ISet<IOwnable<IDebugDrawable>> Pets => _pets; 
    }
}