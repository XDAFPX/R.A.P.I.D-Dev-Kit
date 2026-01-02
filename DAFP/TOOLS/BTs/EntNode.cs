using Bdeshi.Helpers.Utility;
using BDeshi.BTSM;
using DAFP.TOOLS.ECS;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.BTs
{
    public abstract class EntNode : BtNodeBase
    {
        internal BlackBoard BlackBoard;

        internal IEntity GetSelf()
        {
            return BlackBoard.GetSelf();
        }

        internal IEntity GetTarget()
        {
            return BlackBoard.GetTarget();
        }

        internal abstract bool HasRequiredMemory();

        internal EntNode(BlackBoard bb)
        {
            BlackBoard = bb;
        }
    }
}