
using UnityEngine;
using UnityEngine.InputSystem;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.BuiltIn
{
    [RequireComponent(typeof(PlayerInput))]
    public abstract class AbstractGamePlayer : StateDrivenEntity, IGamePlayer
    {
        // public abstract PlayerID PlayerID { get; }
    }
}