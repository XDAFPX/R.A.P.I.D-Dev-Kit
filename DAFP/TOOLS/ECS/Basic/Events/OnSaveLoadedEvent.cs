using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnSaveLoadedEvent : ISaveSystemEvent
    {
        public OnSaveLoadedEvent(ISaveSystem system, ISerializer<IEntity> serializer, ISerializationService service)
        {
            System = system;
            Serializer = serializer;
            Service = service;
        }

        public ISaveSystem System { get; }
        public ISerializer<IEntity> Serializer { get; }
        public ISerializationService Service { get; }

        public override string ToString()
        {
            return GameUtils.FormatLog(nameof(ISaveSystem), $"Save () was loaded, with system ({System})"); //TODO include a Save object for logging and for everything else
        }
    }
}