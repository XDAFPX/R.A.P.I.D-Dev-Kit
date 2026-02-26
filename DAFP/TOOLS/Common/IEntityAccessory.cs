using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS;

namespace RapidLib.DAFP.TOOLS.Common
{
    public interface IEntityAccessory : IOwnedBy<IEntity>, IPetOf<IEntity,IEntityAccessory>
    {
        
    }
}