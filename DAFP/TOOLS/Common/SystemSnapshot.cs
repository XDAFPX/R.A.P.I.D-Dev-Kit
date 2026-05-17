using System;
using UnityEngine;

namespace RapidLib.DAFP.TOOLS.Common
{
    public struct SystemSnapshot
    {
        public DateTime Timestamp { get; init; }

        public string OperatingSystem { get; init; }
        public int ProcessorCount { get; init; }

        public string DeviceModel { get; init; }
        public string DeviceName { get; init; }
        public string GraphicsDevice { get; init; }

        public int SystemMemoryMB { get; init; }
        public int GraphicsMemoryMB { get; init; }

        public static SystemSnapshot Capture()
        {
            return new SystemSnapshot
            {
                Timestamp = DateTime.Now,

                OperatingSystem = Environment.OSVersion.ToString(),
                ProcessorCount = Environment.ProcessorCount,

                DeviceModel = SystemInfo.deviceModel,
                DeviceName = SystemInfo.deviceName,
                GraphicsDevice = SystemInfo.graphicsDeviceName,

                SystemMemoryMB = SystemInfo.systemMemorySize,
                GraphicsMemoryMB = SystemInfo.graphicsMemorySize
            };
        }
    }
}