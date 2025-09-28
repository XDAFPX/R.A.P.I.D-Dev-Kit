using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalDifficultyConfigStateHandler : GlobalConfigStateHandler, IEventInvoker, IGlobalSettingsSavable
    {
        [Inject]
        public UniversalDifficultyConfigStateHandler([Inject(Id = "DefaultDifficulty")] string defaultState,
            [Inject(Id = "DefaultDifficultyDomain")]
            string domain, [Inject(Id = "GlobalStateBus")] IEventBus bus) : base(defaultState,bus)
        {
            this.domain = domain;
        }


        protected override IConfigState[] GetPreBuildStates()
        {
            return new[]
                { new BasicConfigState("Default"), new BasicConfigState("Hard"), new BasicConfigState("Easy") };
        }

        public override Dictionary<string, object> Save()
        {
            return new Dictionary<string, object>() { { "CurrentState", Current().StateName } };
        }

        public override void Load(Dictionary<string, object> save)
        {
            if (save.TryGetValue("CurrentState", out var _value))
            {
                ResetToDefault();
                PushState(new StateChangeRequest<IConfigState>(GetState(_value as string), 0, "SaveSys"));
            }
        }

        private readonly string domain;

        public override string GetDomain()
        {
            return domain;
        }

        public void Invoke<TEvent>(in TEvent e, in ISubscriber listener)
        {
        }
    }
}