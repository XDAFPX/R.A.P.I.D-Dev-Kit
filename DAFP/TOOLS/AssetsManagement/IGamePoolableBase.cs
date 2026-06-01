namespace DAFP.TOOLS.AssetManagement
{
    public interface IGamePoolableBase 
    {
        string UName { get; }
        string Prefix { get; }
        void OnSpawn();
    }
}