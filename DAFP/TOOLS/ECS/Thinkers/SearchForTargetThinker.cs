using System;
using System.Collections.Generic;
using DAFP.TOOLS.ECS.BuiltIn;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Environment.Filters;
using Optional;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Thinkers
{
    [CreateAssetMenu(menuName = "R.A.P.I.D/BuiltIn/Thinker/" + nameof(SearchForTargetThinker),
        fileName = nameof(SearchForTargetThinker))]
    public class SearchForTargetThinker : BaseThinker
    {
        [SerializeField] private EntityFilter EntityFilter;

        protected override void InternalInitialize(IEntity host)
        {
        }

        protected override void InternalTick(IEntity host, ITickerBase ticker)
        {
            if (host is ICommonEntityInterface.IEntTargetDetectable _targetDetectable)
            {
                _targetDetectable.Target = _targetDetectable.ScanForTarget(EntityFilter).Some();
            }
        }

        protected override void InternalDispose(IEntity host)
        {
        }

        protected override IEnumerable<IDebugDrawer> SetupDebugDrawers(IEntity host)
        {
            return ArraySegment<IDebugDrawer>.Empty;
        }
    }
}