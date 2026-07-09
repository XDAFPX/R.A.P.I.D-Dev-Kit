using System;
using System.Reflection;
using MessagePipe;
using Microsoft.Extensions.Logging;
using UnityEngine;
using Zenject;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DAFP.TOOLS.ECS.Basic
{
    public sealed class EventLoggerFilter<T> : MessageHandlerFilter<T>
    {
        [Inject] private ILogger logger;

        private static readonly LogLevel _level = typeof(T)
                                                      .GetCustomAttribute<LogLevelAttribute>()?.Level
                                                  ?? LogLevel.Information;

        public override void Handle(T message, Action<T> next)
        {
            logger.Log(_level, $"{message} <{typeof(T).Name}>");
            next(message);
        }
    }
}