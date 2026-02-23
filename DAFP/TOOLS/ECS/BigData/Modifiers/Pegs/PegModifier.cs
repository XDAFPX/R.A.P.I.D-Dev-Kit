using System;
using DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Modifiers.Pegs
{
    public abstract class PegModifier : IEntityOwnedBy, IDisposable
    {
        protected IEntity Owner;

        [field : SerializeField]public int Priority { get; set; }
        public IStatBase Peg { get; set; }

        public IEntity GetCurrentOwner()
        {
            return Owner;
        }

        public void ChangeOwner(IEntity newOwner)
        {
            Owner = newOwner;
        }

        public void Dispose()
        {
        }
    }
}