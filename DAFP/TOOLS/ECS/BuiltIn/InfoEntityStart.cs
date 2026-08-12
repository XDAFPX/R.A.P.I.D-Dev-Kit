using System.Collections.Generic;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.ECS.Environment;
using TNRD;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class InfoEntityStart : EntFactory
    {
        [field: SerializeField]
        protected override SerializableInterface<IAsyncFactory<IEnumerable<IEntity>>> Factory { get; set; }

        [Inject] protected override ISpawnPointManager Manager { get; set; }
    }
}