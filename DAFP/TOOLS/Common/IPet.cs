using System.Collections.Generic;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.Common
{
    public interface IPet :IOwnable
    {
        List<IEntity> Owners { get; }
        IEntity GetExOwner();

    }
}