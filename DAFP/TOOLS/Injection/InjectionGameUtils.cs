using DAFP.TOOLS.ECS.Basic;
using MessagePipe;
using Zenject;

namespace DAFP.TOOLS.Injection
{
    public static class InjectionGameUtils
    {
            public static void BindLoggedMessageBrocker<T>(
                this DiContainer container, MessagePipeOptions options)
            {
                container.BindMessageBroker<T>(options);
                container.Bind<EventLoggerFilter<T>>().AsCached();
                options.AddGlobalMessageHandlerFilter<EventLoggerFilter<T>>();
            }
    }
}