using System;
using DAFP.TOOLS.ECS.Basic.Events;
using MessagePipe;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    internal sealed class WorldEntityInitializer : IInitializable, IDisposable, IMessageHandler<OnEntityReadyToInitializeEvent> 
    {//-- responsible  for initializing all entities 
        [Inject] private World world;
        [Inject] private ISubscriber<OnEntityReadyToInitializeEvent> register;
        private IDisposable sub;

        public void Initialize()
        {
            sub = register.Subscribe(this);
        }

        public void Dispose() => sub?.Dispose();

        void IMessageHandler<OnEntityReadyToInitializeEvent>.Handle(OnEntityReadyToInitializeEvent message)
        {
            
            world.HandleEntityInitialization(message.Entity);
        }
    }
}