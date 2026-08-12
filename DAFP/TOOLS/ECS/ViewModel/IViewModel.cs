using System;
using Cysharp.Threading.Tasks;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Environment.DamageSys;
using DAFP.TOOLS.ECS.Environment.TriggerSys.HitBoxSys;
using Optional;
using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.ViewModel
{
    public interface IViewModel : IPetOf<IEntity, IViewModel>, INameable, ISwitchable
    {
        public IViewModel InitOwner(IEntity owner);
        public HurtGroup<IEntity> GetHurtGroup(IEntity owner);

        public Option<UniTask> Resolve(IAnimAction action);
    }

    public interface IProceduralView : IDirectionProvider
    {
        public IStat<float> LimbPower { get; }
        public IStat<float> LimbAcceleration { get; }
        public float WiggleTail { get; set; }
        public event Action OnSpineCurveUp ;
    }

    public interface IGlitchedView : IViewModel
    {
        public float Glitchiness { get; set; }

    }
}