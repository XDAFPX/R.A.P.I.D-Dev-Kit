using System;
using System.Threading.Tasks;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface ISaveSystem
    {
        public void LoadAll(ISerializationDataService saveDataService, ISerializer serializer,
            ISerializer<GameMetaData> metaSerializer,  int slot);

        public void SaveAll(ISerializationDataService saveDataService, ISerializer serializer,
            ISerializer<GameMetaData> metaSerializer, int slot);

        public void DeleteSave(ISerializationDataService dataService, int slot);

    }
    

}