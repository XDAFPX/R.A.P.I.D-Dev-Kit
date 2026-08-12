using System;
using UnityEngine;

namespace DAFP.TOOLS.AssetManagement
{
    public interface IGamePoolable<out TY> :  IGamePoolableBase where TY : Component
    {
        TY ResetObj();
        TY Get();
    }

}