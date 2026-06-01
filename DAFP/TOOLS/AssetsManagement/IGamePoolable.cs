using System;
using UnityEngine;

namespace DAFP.TOOLS.AssetManagement
{
    public interface IGamePoolable<TY> : IDisposable, IGamePoolableBase where TY : Component
    {
        TY ResetObj();
        TY Get();
    }

}