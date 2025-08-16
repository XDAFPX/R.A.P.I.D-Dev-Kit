using System;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.BigData.GlobalModifiers
{
public abstract class GlobalWhiteBoard<T, TAttribute> : WhiteBoard<T>, IGlobalStat<T, TAttribute>
    where TAttribute : Attribute
{
    public override T DefaultValue { get; set; }
    public override T MaxValue { get; set; }
    public override T MinValue { get; set; }

    protected override void ResetInternal()
    {
        Value = DefaultValue;
    }

    public override void SetToMax()
    {
        Value = MaxValue;
    }

    public override void SetToMin()
    {
        Value = MinValue;
    }

    protected override T GetValue(T ProcessedValue)
    {
        return ProcessedValue;
    }

    protected override void OnStart()
    {
        foreach (var _entityComponent in Host.Components)
        {
            if (_entityComponent is IStat<T> _stat && !ReferenceEquals(_entityComponent, this))
                ApplyModifiers(_stat);
        }
    }

    public abstract StatModifier<T>[] GetModifiers();

    public void ApplyModifiers(IStat<T> stat)
    {
        if (stat.GetType().GetCustomAttributes(typeof(TAttribute), true).Length > 0)
        {
            foreach (var _statModifier in GetModifiers())
            {
                stat.AddModifier(_statModifier);
            }
        }
    }
}
}