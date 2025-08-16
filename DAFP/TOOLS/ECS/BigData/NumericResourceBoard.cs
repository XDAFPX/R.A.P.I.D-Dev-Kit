using System;
using Archon.SwissArmyLib.Utils.Editor;
using UnityEngine;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData
{
    public abstract class NumericResourceBoard<T> : ResourceBoard<T> where T : IComparable<T>
    {
        [ReadOnly(OnlyWhilePlaying = true), SerializeField]
        protected T defaultValue;

        [SerializeField]
        protected T maxValue;

        public override T DefaultValue
        {
            get => defaultValue;
            set => defaultValue = value;
        }

        public override T MaxValue
        {
            get
            {
                T temp = maxValue;
                StatModifiers.Sort();
                foreach (var modifier in StatModifiers)
                {
                    temp = modifier.Apply(temp);
                }
                return temp;
            }
            set => maxValue = value;
        }

        public override T MinValue
        {
            get => default(T);
            set { }
        }




        public override string GetFormatedMaxValue() => FormatValue(MaxValue);

        public override string GetFormatedCurValue() => FormatValue(Value);

        protected virtual string FormatValue(T val)
        {
            if (val is float f)
                return Math.Round(f, 1).ToString();
            if (val is double d)
                return Math.Round(d, 1).ToString();
            return val.ToString();
        }

    }
}