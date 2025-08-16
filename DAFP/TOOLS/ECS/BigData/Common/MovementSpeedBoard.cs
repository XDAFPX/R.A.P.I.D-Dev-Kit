using DAFP.TOOLS.ECS.BigData.GlobalModifiers;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Common
{
    [RequireComponent(typeof(AccelerationBoard))]
    [RequireComponent(typeof(DecelerationBoard))]
    [SpeedModdable]
    public class MovementSpeedBoard : FloatResourceBoard
    {
        public override bool SyncToBlackBoard => true;
    }
}