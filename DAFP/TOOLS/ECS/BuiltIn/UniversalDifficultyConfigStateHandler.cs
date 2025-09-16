using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalDifficultyConfigStateHandler : GlobalConfigStateHandler, IEventInvoker,IGlobalSettingsSavable
    {
        private readonly ISaveSystem saveSystem;

        [Inject]
        public UniversalDifficultyConfigStateHandler([Inject(Id = "DefaultDifficulty")] string defaultState,
            ISaveSystem saveSystem,[Inject(Id = "DefaultDifficultyDomain")] string domain,GlobalStates states) : base(defaultState,states)
        {
            this.saveSystem = saveSystem;
            this.domain = domain;
        }

        protected override IEventBus CustomBus => saveSystem.Bus;


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