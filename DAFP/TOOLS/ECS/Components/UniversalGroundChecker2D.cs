using DAFP.TOOLS.ECS.BigData.Common;
using PixelRouge.Colors;
using PixelRouge.Inspector;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Components
{
    [RequireComponent(typeof(GroundedBoard))]
    public class UniversalGroundChecker2D : EntityComponent

    {
        [SerializeField] private LayerMask CheckMask;
        [SerializeField] private GroundCheckPosition[] Positions;

        protected override void OnTick()
        {
            var _isGrounded = false;

            foreach (var check in Positions)
            {
                var pos = check.Pos.position;
                var box = check.BoundingBox;

                // Step 4a: if width == 0, perform a downward raycast
                if (Mathf.Approximately(box.x, 0f))
                {
                    RaycastHit2D hit = Physics2D.Raycast(
                        pos,
                        Vector2.down,
                        box.y,
                        CheckMask
                    );

                    if (hit.collider != null)
                    {
                        _isGrounded = true;
                        break;
                    }
                }
                // Step 4b: otherwise use an OverlapBox at the position
                else
                {
                    Collider2D hit = Physics2D.OverlapBox(
                        pos,
                        new Vector2(box.x, box.y),
                        0f,
                        CheckMask
                    );

                    if (hit != null)
                    {
                        _isGrounded = true;
                        break;
                    }
                }
            }

            // Step 5: update the GroundedBoard component
            GetEntComponent<GroundedBoard>().Value = _isGrounded;
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
            if (Positions == null)
                return;

            Gizmos.color = ColorsForUnity.Orangered;
            foreach (var check in Positions)
            {
                if (check.Pos == null)
                    continue;

                Vector3 pos = check.Pos.position;
                Vector3 box = check.BoundingBox;

                if (Mathf.Approximately(box.x, 0f))
                {
                    // draw downward ray
                    Vector3 end = pos + Vector3.down * box.y;
                    Gizmos.DrawLine(pos, end);
                    Gizmos.DrawSphere(end, 0.05f);
                }
                else
                {
                    // draw wireframe box
                    Gizmos.DrawWireCube(pos, new Vector3(box.x, box.y, 0f));
                }
            }
        }
#endif
    }

    [System.Serializable]
    internal struct GroundCheckPosition
    {
        public Transform Pos;

        [HelpBox("The bounding box for boxed Ground Check. Set width to 0 to turn in to raycast checking")]
        public Vector3 BoundingBox;
    }
}