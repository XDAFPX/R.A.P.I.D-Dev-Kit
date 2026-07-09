using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.AssetManagement
{
    public interface IGamePoolableBase : IResetable
    {
        string UName { get; }
        string Prefix { get; }
        void OnSpawn();
    }
}