using UnityEngine;

namespace DAFP.TOOLS.Common
{
    public interface IGameObjectContainer : IGameObjectProvider
    {
        public void SetWorldRepresentation(GameObject Gameobject);
    }
}