using DAFP.TOOLS.ECS.BuiltIn;
using UnityEngine.InputSystem;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.Components
{
    public abstract class AbstractUniversalInputController : EntityComponent, IInputController
    {

        [field: GetComponentCache]public PlayerInput Input { get; }
    }
}