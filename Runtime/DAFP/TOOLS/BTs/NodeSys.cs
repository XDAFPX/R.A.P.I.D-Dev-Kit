using Bdeshi.Helpers.Utility;
using BDeshi.BTSM;
using DAFP.Game.Utill;
using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.Game.Effects;
using DAFP.Game.Enemies.Pathfinding;
using DAFP.GAME.Essential;
using DAFP.Game.Projectiles;
using DAFP.TOOLS.ECS;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.BTs
{
    public abstract class EnemyNode : BtNodeBase
    {
        internal BlackBoard BlackBoard;

        internal Entity GetSelf()
        {
            return BlackBoard.GetSelf();
        }

        internal Entity GetTarget()
        {
            return BlackBoard.GetTarget();
        }

        internal abstract bool HasRequiredMemory();

        internal EnemyNode(BlackBoard bb)
        {
            BlackBoard = bb;
        }
    }

}