using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData.Modifiers.Pegs
{
    public abstract class PegModifier : IPetOf<IEntity,PegModifier>, IDisposable
    {
        protected IEntity Owner;

        [field : SerializeField]public int Priority { get; set; }
        public IStatBase Peg { get; set; }

        public List<IEntity> Owners { get; } = new();

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