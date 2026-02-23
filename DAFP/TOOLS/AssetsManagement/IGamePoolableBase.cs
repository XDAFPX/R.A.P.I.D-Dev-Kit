namespace DAFP.GAME.Assets
{
    public interface IGamePoolableBase : IPoolComponentProvider
    {
        string UName { get; }
        string Prefix { get; }
        void OnSpawn();
    }
}