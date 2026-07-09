using Cysharp.Threading.Tasks;
using DAFP.TOOLS.AssetManagement;
using DAFP.TOOLS.ECS.Basic.Events;
using MessagePipe;
using Zenject;

namespace DAFP.TOOLS.ECS.Services
{
    internal interface IObjectDestroyPreparer : IAsyncFactory<object, object>
    //--the after injection and preparement of an object
    {
    }

    internal class InitialObjectDestroyPreparer : IObjectDestroyPreparer
    {
        [Inject] private IAsyncPublisher<OnObjectDestroyedAsyncEvent> destroyedEvent;
        [Inject] private IPublisher<OnObjectDeregister> deregisterEvent;

        public async UniTask<object> Create(object param1)
        {
            await destroyedEvent.PublishAsync(new OnObjectDestroyedAsyncEvent(param1));
            deregisterEvent.Publish(new OnObjectDeregister(param1));
            return param1;
        }
    }
}