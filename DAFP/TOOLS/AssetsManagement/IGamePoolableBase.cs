namespace DAFP.GAME.Assets
{
    public interface IGamePoolableBase : IPoolComponentProvider
    {
        void OnSpawn();
    }
}