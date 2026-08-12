using System;
using Archon.SwissArmyLib.Utils;
using DAFP.TOOLS.Common.TextSys;
using Microsoft.Extensions.Logging;
using R3;
using UnityEngine;
using Zenject;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace DAFP.TOOLS.ECS.Basic
{
    public class StandardDebugLogger : ILogger
    {
        [Inject] private LogLevel level;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if(logLevel < level)
                return;
            switch (logLevel)
            {
                case LogLevel.Trace:
                    Debug.Log(new TextSpan(formatter(state, exception)).Color_RT(Color.blueViolet.ToHex()));
                    break;
                case LogLevel.Debug:
                    Debug.Log(new TextSpan(formatter(state, exception)).Color_RT(Color.coral.ToHex()));
                    break;
                case LogLevel.Information:
                    Debug.Log(formatter(state, exception));
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(formatter(state, exception));
                    break;
                case LogLevel.Error:
                    Debug.LogError(formatter(state, exception));
                    break;
                case LogLevel.Critical:
                    Debug.LogError(new TextSpan(formatter(state, exception)).Color_RT(Color.violetRed.ToHex()));
                    break;
                case LogLevel.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state) where TState : notnull
        {
            return Disposable.Empty;
        }
    }
}