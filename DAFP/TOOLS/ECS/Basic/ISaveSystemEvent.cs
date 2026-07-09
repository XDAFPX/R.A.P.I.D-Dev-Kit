using DAFP.TOOLS.ECS.Serialization;

namespace DAFP.TOOLS.ECS.Basic
{
    public interface ISaveSystemEvent
    {
        ISaveSystem System { get; }

        ISerializer<IEntity> Serializer { get; } //TODO change to a system of Serializers of different kinds
        ISerializationService Service { get; } 
    }
}