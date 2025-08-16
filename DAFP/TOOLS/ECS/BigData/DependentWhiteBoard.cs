using DAFP.TOOLS.ECS.BigData.Health;
using UnityEngine;

namespace DAFP.TOOLS.ECS.BigData
{
    /// <summary>
    /// A WhiteBoard whose MaxValue and MinValue are determined by other WhiteBoards.
    /// </summary>
    public abstract class DependentWhiteBoard<T, TMax, TMin> : WhiteBoard<T>, IDependentStat<T>
        where TMax : WhiteBoard<T>, IStatDependent<T> where TMin : WhiteBoard<T>, IStatDependent<T>
    {
        protected override void OnInitializeInternal()
        {
            
            MaxSource = Host.GetEntComponent<TMax>();
            MinSource = Host.GetEntComponent<TMin>();

            if (MaxSource == null || MinSource == null)
            {
                Debug.LogError($"YOU DUMB ASS DID YOU jUST TRIED TO MAkE a " +
                               $"DEPENDENT WHITE BOARD({GetType().FullName} , world name ({GetWorldRepresentation().name}))" +
                               $" WITHOUT ITS OTHER BOARDS!!! (Required : {typeof(TMax).FullName}) and {typeof(TMin).FullName}");
                return;
            }
            MaxSource.Register(this);
            MinSource.Register(this);
            ResetToDefault();
        }


        public IStatDependent<T> MaxSource { get; set; }

        public IStatDependent<T> MinSource { get; set; }

        public override T MaxValue
        {
            get => MaxSource != null ? MaxSource.Value : default;
            set
            {
                if (MaxSource != null)
                    MaxSource.Value = value;
            }
        }

        public override T MinValue
        {
            get => MinSource != null ? MinSource.Value : default;
            set
            {
                if (MinSource != null)
                    MinSource.Value = value;
            }
        }

        // Make DefaultValue abstract to avoid duplicate backing fields
        public override abstract T DefaultValue { get; set; }

        protected override T GetValue(T ProcessedValue)
        {
            return ProcessedValue;
        }

        public override void SetToMax()
        {
            if (MaxSource != null)
                Value = MaxSource.Value;
        }

        public override void SetToMin()
        {
            if (MinSource != null)
                Value = MinSource.Value;
        }

        protected override void ResetInternal()
        {
            InternalValue = DefaultValue;
        }
    }
}