using DAFP.TOOLS.ECS.Basic.Events;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    internal sealed class WorldDecisionMaker : IInitializable // responsible for sending out the decision event
    {
        [Inject] private MessagePipe.IPublisher<OnWorldDecisionEvent> worldDecisionEvent;
        [Inject] private World world;

        public void Initialize()
        {
            worldDecisionEvent.Publish(new(world)); //TODO handle scene transitions
        }
    }
}