using Archon.SwissArmyLib.Utils.Editor;

namespace RapidLib.DAFP.TOOLS.Common
{
    using UnityEngine;
    using UnityEngine.AddressableAssets;


    [System.Serializable]
    public class AddressableEntry
    {
        public AssetReference assetRef; // drag/drop in inspector, no text
        [ReadOnly][SerializeField] private string cachedAddress; // this is what your spawn system uses

        public string Address
        {
            get => cachedAddress;
            set => cachedAddress= value;
        }
    }
}