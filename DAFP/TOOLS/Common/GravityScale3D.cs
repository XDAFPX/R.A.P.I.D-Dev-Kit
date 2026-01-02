using UnityEngine;

namespace DAFP.TOOLS.Common
{
    public class GravityScale3D : MonoBehaviour
    {
        // Gravity Scale editable on the inspector
        // providing a gravity scale per object

        public float gravityScale = 1.0f;

        // Global Gravity doesn't appear in the inspector. Modify it here in the code
        // (or via scripting) to define a different default gravity for all objects.

        public static float globalGravity = -9.81f;

        private Rigidbody m_rb;

        private void OnEnable()
        {
            m_rb = GetComponent<Rigidbody>();
            m_rb.useGravity = false;
        }

        private void FixedUpdate()
        {
            var gravity = globalGravity * gravityScale * Vector3.up;
            m_rb.AddForce(gravity, ForceMode.Acceleration);
        }
    }
}