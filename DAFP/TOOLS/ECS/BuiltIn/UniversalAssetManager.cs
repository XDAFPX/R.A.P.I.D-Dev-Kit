using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.AssetManagement;
using Zenject;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    public class UniversalAssetManager : AssetManager
    {
        private readonly IEnumerable<IAssetPoolBase> assetPools;
        
        [Inject]public UniversalAssetManager(IAssetFactory assetFactory,  IEnumerable<IAssetPoolBase> assetPools) : base(assetFactory)
        {
            this.assetPools = assetPools;
        }

        protected override HashSet<IAssetPoolBase> GetPools()
        {
            
            return assetPools.ToHashSet();
        }
    }
}