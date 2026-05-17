using System;
using System.Collections;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using R3;
using UGizmo;
using UnityEngine;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public interface IDebugSubSys : IOwnedBy<IDebugSys<IGlobalGizmos, IConsoleMessenger>>
    {
    }

    public struct DebugValue : INameable, IEquatable<DebugValue>
    {
        public Func<string> Stream;
        public string Name { get; set; }

        public bool Equals(DebugValue other)
        {
            return   Name == other.Name;
        }

        public override bool Equals(object obj)
        {
            return obj is DebugValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Stream, Name);
        }
    }

    public interface IDebugSys<out TGizmos, out TMessenger> : IDrawable, IOwnerOf<IDebugDrawable>,
        Zenject.ITickable, IOwnerOf<IDebugSubSys>
        where TGizmos : IGlobalGizmos where TMessenger : IConsoleMessenger
    {
        public TGizmos Gizmos { get; }
        public TMessenger Messenger { get; }
        public IList<DebugDrawLayer> Layers { get; }
        public DebugDrawLayer GetSharedLayer { get; }
        public void AddDebugValue(DebugValue value);
        public void RemoveDebugValue(string value);

        public void Log(INameable system, object message)
        {
            var systemname = system != null ? system.Name : "0";
            var fullmessage = $"[{systemname}]: {message}";

            if (system == null)
                Debug.LogWarning(fullmessage);
            else
                Debug.Log(fullmessage);

            Messenger.Print(CompText.Literal(fullmessage));
        }


        void Zenject.ITickable.Tick()
        {
            Messenger.Tick();
            Draw();
        }

        void IDrawable.Draw()
        {
            foreach (var _ownable in ((IOwnerOf<IDebugDrawable>)this).Pets)
                _ownable.Draw();
        }
    }
}