using System;
using Bdeshi.Helpers.Utility;
using BDeshi.BTSM;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace DAFP.TOOLS.BTs
{
    public abstract class EntNode : BtNodeBase
    {
        internal BlackBoard BlackBoard => host.Memory;
        private IEntity host;

        internal IEntity GetSelf()
        {
            return host;
        }


        public sealed override BtStatus InternalTick()
        {
            if (!HasRequiredMemory()) return BtStatus.Failure;
            try
            {
                return Work();
            }
            catch (Exception _e)
            {
                Debug.LogError(_e);
                return BtStatus.Failure;
            }
        }

        protected abstract BtStatus Work();

        protected abstract bool HasRequiredMemory();

        internal EntNode(IEntity ent)
        {
            ent.ThrowIfNull(nameof(ent));
            host = ent;
        }
    }
}