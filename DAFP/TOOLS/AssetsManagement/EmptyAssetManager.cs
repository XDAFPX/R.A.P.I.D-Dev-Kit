using System;
using System.Collections.Generic;
using Zenject;

namespace DAFP.TOOLS.AssetManagement
{ 
    public class EmptyAssetManager : AssetManager
    {
        protected override HashSet<IAssetPoolBase> GetPools()
        {
            return new HashSet<IAssetPoolBase>();
        }


        [Inject]public EmptyAssetManager(IAssetFactory assetFactory) : base(assetFactory)
        {
        }
    }
}