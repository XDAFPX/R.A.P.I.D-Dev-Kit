using DAFP.TOOLS.ECS.Basic.Events;
using DAFP.TOOLS.ECS.BuiltIn;
using MessagePipe;
using Zenject;

namespace DAFP.TOOLS.ECS.Environment
{
    public class WorldDecisionMaker : EmptyEntity
    {
        [Inject] private IPublisher<OnWorldDecisionEvent> worldDecision;
        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            worldDecision.Publish(new OnWorldDecisionEvent(World));
        }
    }
}