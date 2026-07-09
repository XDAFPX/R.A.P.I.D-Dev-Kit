using System;
using System.Threading.Tasks;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface ISaveSystem
    {
        public Task LoadAll(ISerializationService saveService, ISerializer<IEntity> serializer,
            IMetaSerializer metaSerializer, Action OnEnd, int slot);

        public void SaveAll(ISerializationService saveService, ISerializer<IEntity> serializer,
            IMetaSerializer metaSerializer, int slot);

        public void DeleteSave(ISerializationService service, int slot);

        public void TryChangeCurrentScene(ISerializationService serializationService, IMetaSerializer serializer,
            int scene, int slot);
    }

}