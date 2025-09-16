using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface IMetaSerializer
    {
        public Task LoadMetaData(Dictionary<string, object> save); 
        public Dictionary<string, object> SaveMetaData();
    }
}