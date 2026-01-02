using System;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [CreateAssetMenu(menuName = "R.A.P.I.D/Filter/" + nameof(EntityFilter), fileName = nameof(EntityFilter))]
    public class EntityFilter : MultiFilterSO<IEntity>
    {
    }
    [Serializable]
    public class EntityMultiFilter : MultiFilter<IEntity>
    {
    }
}