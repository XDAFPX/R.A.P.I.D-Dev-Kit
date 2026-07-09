using System;
using System.Collections.Generic;
using DAFP.TOOLS.ECS.Basic;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Environment.Filters;
using Optional;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Thinkers
{
    [CreateAssetMenu(menuName = "R.A.P.I.D/BuiltIn/Thinker/" + nameof(SearchForTargetThinker),
        fileName = nameof(SearchForTargetThinker))]
    public class SearchForTargetThinker : Brain
    {
        [SerializeField] private EntityFilter EntityFilter;

        protected override void InternalStart(IEntity host)
        {
        }

        protected override void InternalTick(IEntity host, ITickerBase ticker)
        {
            if (host is ITargetDetectable _targetDetectable)
            {
                _targetDetectable.Target.Value = _targetDetectable.ScanForTarget(EntityFilter).Value;
            }
        }

        protected override void InternalEnd(IEntity host)
        {
        }

        protected override IEnumerable<IDebugDrawer> SetupDebugDrawers(IEntity host)
        {
            return ArraySegment<IDebugDrawer>.Empty;
        }
    }
}