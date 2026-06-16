using System.Collections.Generic;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.Environment
{
    public interface ISpawnPoint :  IAsyncFactory<IEntity>,
        IOwnedBy<ISpawnPointManager>
    {
    }
}