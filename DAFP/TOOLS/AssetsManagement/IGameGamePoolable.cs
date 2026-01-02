using System;
using UnityEngine;

namespace DAFP.GAME.Assets
{
    public interface IGameGamePoolable<TY> : IDisposable, IGamePoolableBase where TY : Component
    {
        string UName { get; }
        TY ResetObj();
        TY Get();
    }

    public interface IPoolComponentProvider
    {
        Component Self();
    }
}