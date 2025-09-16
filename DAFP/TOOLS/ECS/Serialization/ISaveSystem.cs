using System;
using System.Threading.Tasks;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Serialization
{
    public interface ISaveSystem
    {
        public Task LoadAll(ISerializationService saveService, ISerializer serializer,IMetaSerializer metaSerializer ,Action OnEnd);

        public SerializationBus Bus { get; }
        public void SaveAll(ISerializationService saveService, ISerializer serializer, IMetaSerializer metaSerializer);
    }

    public struct OnSaveMadeOrLoaded
    {
        private ISerializationService service;
        private ISerializer serializer;
        private string fullPath;

        public OnSaveMadeOrLoaded(string fullPath, ISerializer serializer, ISerializationService service, bool IsSaved)
        {
            this.fullPath = fullPath;
            this.serializer = serializer;
            this.service = service;
        }
    }
}