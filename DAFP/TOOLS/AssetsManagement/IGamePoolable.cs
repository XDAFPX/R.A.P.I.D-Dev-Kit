using System;
using UnityEngine;

namespace DAFP.GAME.Assets
{
    public interface IGamePoolable<TY> : IDisposable, IGamePoolableBase where TY : Component
    {
        TY ResetObj();
        TY Get();
    }

    public interface IPoolComponentProvider
    {
        Component Self();
    }
}