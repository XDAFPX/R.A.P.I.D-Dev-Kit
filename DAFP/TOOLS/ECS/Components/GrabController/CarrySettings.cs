namespace DAFP.TOOLS.ECS.Components.GrabController
{
    using UnityEngine;

    /// <summary>
    /// Optional component that defines preferred carry orientation and distance.
    /// Attach this to props that should align when picked up.
    /// </summary>
    public class CarrySettings : MonoBehaviour
    {
        [Tooltip("If true, use preferred carry angles instead of free rotation.")]
        public bool usePreferredAngles = false;

        [Tooltip("Preferred local angles when carried (relative to player forward).")]
        public Vector3 preferredCarryAngles = Vector3.zero;

        [Tooltip("Offset from camera when held.")]
        public float distanceOffset = 0f;
    }
}