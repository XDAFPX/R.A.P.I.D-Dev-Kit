using System.Collections.Generic;
using DAFP.TOOLS.ECS.Serialization;

namespace DAFP.TOOLS.ECS.GlobalState
{
    public class SettingsGlobalBoard : GlobalBlackBoard,IGlobalSettingsSavable
    {
        public SettingsGlobalBoard(Dictionary<string, object> data) : base(data)
        {
        }
    }
}