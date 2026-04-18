using System;

namespace DAFP.TOOLS.ECS.BigData
{
    public struct DummyStatContainer : IStatContainer
    {
        public IStatContainer MarkAsDirty() => this;
        public IStatContainer InvalidateCache() => this;
        public IStatContainer Construct(IEntity parent) => this;

        public IStat<T> Get<T>(string name, Func<IStat<T>> fallback) => fallback.Invoke();

        public bool Has(string statName) => false;

        public bool Has(string statName, out IStatBase stat)
        {
            stat = null;
            return false;
        }

        public bool Has(StatInjector.PathBuilder pathBuilder) => false;

        public bool Has(StatInjector.PathBuilder pathBuilder, out IStatBase statBase)
        {
            statBase = null;
            return false;
        }

        public IStatContainer Add(IStatBase stat) => this;
        public bool Add(StatInjector.PathBuilder pathBuilder, IStatBase statToAdd) => false;

        public void Tick()
        {
        }
    }
}