using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAFP.TOOLS.ECS.Serialization
{
    public class SettingsMetaSerializer : IMetaSerializer
    {
        public async Task LoadMetaData(Dictionary<string, object> save)
        {
        }

        public void TryChangeCurrentScene(Dictionary<string, object> save, int scene)
        {
            throw new System.NotImplementedException();
        }

        public Dictionary<string, object> SaveMetaData(Dictionary<string, object> psave)
        {
            return new Dictionary<string, object>();
        }
    }
}