using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface IMetaSerializer
    {
        public Task LoadMetaData(Dictionary<string, object> save);
        public void TryChangeCurrentScene(Dictionary<string, object> save, int scene);
        public Dictionary<string, object> SaveMetaData(Dictionary<string, object> psave);
    }
}