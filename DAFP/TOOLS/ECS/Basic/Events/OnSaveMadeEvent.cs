using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.Serialization;

namespace DAFP.TOOLS.ECS.Basic.Events
{
    public struct OnSaveMadeEvent : ISaveSystemEvent
    {
        public OnSaveMadeEvent(ISaveSystem system, ISerializer<IEntity> serializer, ISerializationService service)
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
            return GameUtils.FormatLog(nameof(ISaveSystem),
                $"Save () was made, with system ({System})"); //TODO include a Save object for logging and for everything else
        }
    }
}