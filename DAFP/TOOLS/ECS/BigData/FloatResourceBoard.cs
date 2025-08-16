using System;
using Archon.SwissArmyLib.Utils.Editor;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    public abstract class FloatResourceBoard : NumericResourceBoard<float> 
    {
        public override void Randomize(float margin01)
        {
            InternalValue = Value.Randomize(margin01);
            MaxValue = Value;
        }

        public override float Percentage => Value / MaxValue; 
        public override float Add(float amount, bool forced = false)
        {
            InternalValue += amount;
            return Value;
        }

        public override float Remove(float amount, bool forced = false)
        {
            InternalValue -= amount;
            return Value;
        }
    }
}