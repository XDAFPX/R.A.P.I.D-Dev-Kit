using System;
using System.Collections;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using Zenject;

namespace DAFP.TOOLS.ECS.BigData
{
    public interface IStatContainer : ITickable,IResetable
    {
        IStatContainer MarkAsDirty();
        IStatContainer InvalidateCache();
        IStatContainer Construct(IHaveStats parent);

        IStat<T> Get<T>(string name, Func<IStat<T>> fallback);

        bool Has(string statName);
        bool Has(string statName, out IStatBase stat);
        bool Has(StatInjector.PathBuilder pathBuilder);
        bool Has(StatInjector.PathBuilder pathBuilder, out IStatBase statBase);

        IStatContainer Add(IStatBase stat);
        IStatContainer Remove(IStatBase stat);
        IEnumerable<IStatBase> All();
        bool Add(StatInjector.PathBuilder pathBuilder, IStatBase statToAdd);
    }
}