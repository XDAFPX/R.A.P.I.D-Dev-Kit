using System;
using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using UnityEngine;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalCursorStateHandler : GlobalCursorStateHandler, IComparable<ISavable>
    {
        [Inject]
        public UniversalCursorStateHandler([Inject(Id = "DefaultCursorState")] string defaultState
            ,[Inject(Id = "GlobalStateBus")] IEventBus bus) : base(defaultState, bus)
        {
        }

        protected override IGlobalCursorState[] GetPreBuildStates()
        {
            return new[] { new BasicCursorState(this, null, "Default") };
        }

        public override Dictionary<string, object> Save()
        {
            return new Dictionary<string, object>() { { "CurrentState", Current().StateName }, { "Visible", Cursor.visible } ,{"Locked" , Cursor.lockState}};
        }

        public override void Load(Dictionary<string, object> save)
        {
            if (save.TryGetValue("CurrentState", out var _value) && save.TryGetValue("Visible",out var _visible) && save.TryGetValue("Locked",out var _locked)&&CursorLockMode.TryParse(_locked as string,out CursorLockMode Locked))
            {
                ResetToDefault();
                PushState(new CursorStateChangeRequest(GetState(_value as string), 0, "SaveSys",
                    Locked, (bool)_visible));
            }
        }

        public int CompareTo(ISavable other)
        {
            return 10;
        }
    }
}