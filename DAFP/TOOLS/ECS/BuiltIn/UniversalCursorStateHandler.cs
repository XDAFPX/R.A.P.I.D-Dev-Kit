using System;
using System.Collections.Generic;
using DAFP.TOOLS.ECS.GlobalState;
using DAFP.TOOLS.ECS.Serialization;
using UnityEngine;
using UnityEventBus;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalCursorStateHandler : CursorStateHandler, IComparable<ISavable>
    {
        [Inject(Id = "DefaultCursorState")]public override IGlobalCursorState Default { get; }

        public int CompareTo(ISavable other)
        {
            return 10;
        }
    }
}