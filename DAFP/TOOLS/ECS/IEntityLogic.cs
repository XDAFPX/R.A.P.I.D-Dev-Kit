using DAFP.TOOLS.ECS.Thinkers;
using Zenject;

namespace DAFP.TOOLS.ECS
{
    internal interface IEntityLogic
    {
        internal void Initialize();
        internal ITickable ThinkerTicker { get; set; }
        internal void SetThinker(IThinker thinker);
    }
}