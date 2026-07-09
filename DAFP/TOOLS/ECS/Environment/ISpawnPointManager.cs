using DAFP.TOOLS.Common;
using Zenject;

namespace DAFP.TOOLS.ECS.Environment
{
    public interface ISpawnPointManager : IOwnerOf<ISpawnPoint>
    {
        public void Resolve();
    }
}