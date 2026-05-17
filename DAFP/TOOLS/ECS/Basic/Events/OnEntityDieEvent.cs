using Optional;
using DAFP.TOOLS.ECS.Environment.DamageSys;
namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnEntityDieEvent
    {
        public OnEntityDieEvent(IEntity receiver, IDamage lethal)
        {
            Receiver = receiver;
            Lethal = lethal;
        }

        public IEntity Receiver { get; }
        public Option<IEntity> Source => Lethal.Info.Source.Author;
        public IDamage Lethal { get; }
    }
}
