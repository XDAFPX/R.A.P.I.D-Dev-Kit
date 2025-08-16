using System;

namespace DAFP.TOOLS.ECS.BigData.GlobalModifiers
{
    public interface IGlobalStat<T, TAtribute> : IStat<T> where TAtribute : Attribute
    {
        StatModifier<T>[] GetModifiers();

        void ApplyModifiers(IStat<T> stat);
    }
}