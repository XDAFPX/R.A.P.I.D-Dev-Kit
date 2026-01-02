using System;
using System.Collections.Generic;
using Zenject;

namespace DAFP.GAME.Assets
{ // Add a custom AssetManager with all of ur assets
    public class EmptyAssetManager : AssetManager
    {
        protected override HashSet<IAssetPoolBase> GetPools()
        {
            return new HashSet<IAssetPoolBase>();
        }

        public override Dictionary<Type, string> AssetPrefixes { get; } = new Dictionary<Type, string>();

        [Inject]public EmptyAssetManager(IAssetFactory.DefaultAssetFactory defaultAssetFactory) : base(defaultAssetFactory)
        {
        }
    }
}