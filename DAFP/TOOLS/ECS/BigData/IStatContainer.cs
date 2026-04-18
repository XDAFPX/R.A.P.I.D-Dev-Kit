using System;
using Zenject;

namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStatContainer : ITickable
    {
        IStatContainer MarkAsDirty();
        IStatContainer InvalidateCache();
        IStatContainer Construct(IEntity parent);

        IStat<T> Get<T>(string name, Func<IStat<T>> fallback);

        bool Has(string statName);
        bool Has(string statName, out IStatBase stat);
        bool Has(StatInjector.PathBuilder pathBuilder);
        bool Has(StatInjector.PathBuilder pathBuilder, out IStatBase statBase);

        IStatContainer Add(IStatBase stat);
        bool Add(StatInjector.PathBuilder pathBuilder, IStatBase statToAdd);

    }
}