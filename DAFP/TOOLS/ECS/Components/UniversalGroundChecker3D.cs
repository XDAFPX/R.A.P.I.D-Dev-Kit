using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.BigData.Common;
using PixelRouge.Colors;
using PixelRouge.Inspector;
using UnityEngine;
using UnityGetComponentCache;

namespace DAFP.TOOLS.ECS.Components
{
    public class UniversalGroundChecker3D : EntityComponent
    {
        [SerializeField] private LayerMask CheckMask;
        [SerializeField] private GroundCheckPosition3D[] Positions;

         [RequireStat][InjectStat("Grounded")] private IStat<bool> groundedBoard;

        protected override void OnTick()
        {
            var isGrounded = false;

            foreach (var check in Positions)
            {
                var pos = check.Pos.position;
                var box = check.BoundingBox;

                // Raycast check when both horizontal extents are zero
                if (Mathf.Approximately(box.x, 0f) && Mathf.Approximately(box.z, 0f))
                {
                    if (Physics.Raycast(pos, Vector3.down, out var hit, box.y, CheckMask))
                    {
                        isGrounded = true;
                        break;
                    }
                }
                else
                {
                    // OverlapBox expects half‐extents
                    var halfExt = new Vector3(box.x, box.y, box.z) * 0.5f;
                    var hits = Physics.OverlapBox(pos, halfExt, Quaternion.identity, CheckMask);
                    if (hits.Length > 0)
                    {
                        isGrounded = true;
                        break;
                    }
                }
            }

            groundedBoard.Value = isGrounded;
        }

        protected override void OnInitialize()
        {
        }

        protected override void OnStart()
        {
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Positions == null) return;
            Gizmos.color = ColorsForUnity.Orangered;

            foreach (var check in Positions)
            {
                if (check.Pos == null) continue;

                var pos = check.Pos.position;
                var box = check.BoundingBox;

                if (Mathf.Approximately(box.x, 0f) && Mathf.Approximately(box.z, 0f))
                {
                    var end = pos + Vector3.down * box.y;
                    Gizmos.DrawLine(pos, end);
                    Gizmos.DrawSphere(end, 0.05f);
                }
                else
                {
                    // draw wireframe box (full size)
                    Gizmos.DrawWireCube(pos, box);
                }
            }
        }
#endif
    }

    [System.Serializable]
    internal struct GroundCheckPosition3D
    {
        public Transform Pos;

        [HelpBox("The bounding box for boxed ground check. Set X and Z to 0 to use a downward ray instead.")]
        public Vector3 BoundingBox;
    }
}