using System;
using DAFP.TOOLS.ECS.Environment;
using System.Linq;
using TNRD;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Environment.Filters
{
    [CreateAssetMenu(menuName = "R.A.P.I.D/Filter/GameObject Filter", fileName = "GameObjectFilter")]
    public class GameObjectFilter : MultiFilterSO<GameObject>
    {
    }

    [Serializable]
    public class GameObjectMultiFilter : MultiFilter<GameObject>
    {
    }
}