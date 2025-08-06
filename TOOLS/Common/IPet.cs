using System.Collections.Generic;
using DAFP.TOOLS.ECS;

namespace DAFP.TOOLS.Common
{
    public interface IPet
    {
        List<Entity> Owners { get; }
        Entity GetCurrentOwner();
        Entity GetExOwner();

        void ChangeOwner(Entity newOwner);
    }
}