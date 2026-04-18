using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.Environment.Filters;
using DAFP.TOOLS.ECS.ViewModel;
using NUnit.Framework;
using RapidLib.DAFP.TOOLS.Common;
using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.TriggerSys
{
    public abstract class FilterActionEntity<TContext> : EmptyEntity
    {
        [SerializeField] protected List<SerializableInterface<IFilter<TContext>>> Filters;
        [SerializeField] protected List<SerializableInterface<IActionUpon<TContext>>> Actions;

        public void Eval(TContext context)
        {
            if (!Filters.All(f => f.Value.Evaluate(context)))
                return;
            act(context);
        }


        public void Eval(IEnumerable<TContext> context)
        {
            var l = new List<TContext>(context);



            foreach (var _filter in Filters.ToValues())
            {
                l = l.FilterThrough(_filter).ToList();
            }

            foreach (var _context in l)
                act(_context);
        }

        private void act(TContext context)
        {
            Actions.ForEach(a => a.Value.Act(context));
        }


        public FilterActionEntity<TContext> AddAction(IActionUpon<TContext> action)
        {
            Actions.Add(new SerializableInterface<IActionUpon<TContext>>(action));
            return this;
        }

        public FilterActionEntity<TContext> RemoveAction(IActionUpon<TContext> action)
        {
            Actions.Remove(new SerializableInterface<IActionUpon<TContext>>(action));
            return this;
        }

        public FilterActionEntity<TContext> AddFilter(IFilter<TContext> filter)
        {
            Filters.Add(new SerializableInterface<IFilter<TContext>>(filter));
            return this;
        }

        public FilterActionEntity<TContext> RemoveFilter(IFilter<TContext> filter)
        {
            Filters.Remove(new SerializableInterface<IFilter<TContext>>(filter));
            return this;
        }

        public override ITicker<IEntity> EntityTicker { get; } = Services.World.EMPTY_TICKER;

        protected override void InitializeInternal()
        {
            foreach (var _filter in Filters.ToValues())
            {
                _filter.Initialize(this);
            }
        }

        protected override void TickInternal()
        {
        }
    }
}