using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAFP.TOOLS.ECS.Serialization
{
    public class SettingsMetaSerializer : IMetaSerializer
    {
        public async Task LoadMetaData(Dictionary<string, object> save)
        {
        }

        public Dictionary<string, object> SaveMetaData()
        {
            return new Dictionary<string, object>();
        }
    }
}