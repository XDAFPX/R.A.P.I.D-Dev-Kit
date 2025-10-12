using System;
using BDeshi.BTSM;
using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS;
using UnityEngine;

namespace DAFP.TOOLS.BTs
{
    public class Nodes
    {
        public class EmptyNode : BtNodeBase
        {
            private readonly BtStatus status;

            public EmptyNode(BtStatus stat = BtStatus.Success)
            {
                status = stat;
            }

            public override void Enter()
            {
            }

            public override void Exit()
            {
            }

            public override BtStatus InternalTick()
            {
                return status;
            }
        }

        public class ActionNode : BtNodeBase
        {
            private readonly System.Action action;

            public ActionNode(System.Action action)
            {
                this.action = action;
            }

            public override void Enter()
            {
            }

            public override void Exit()
            {
            }

            public override BtStatus InternalTick()
            {
                action.Invoke();
                return BtStatus.Success;
            }
        }

        public class LoSCheckNode3D : EnemyNode
        {
            private readonly LayerMask mask;

            public LoSCheckNode3D(BlackBoard bb, LayerMask mask)
                : base(bb)
            {
                this.mask = mask;
            }

            internal override bool HasRequiredMemory()
            {
                return GetSelf() != null && GetTarget() != null;
            }

            public override void Enter()
            {
            }

            public override void Exit()
            {
            }

            public override BtStatus InternalTick()
            {
                if (!HasRequiredMemory())
                    return BtStatus.Failure;

                Vector3 origin = GetSelf().GetWorldRepresentation().transform.position;
                Vector3 direction = (GetTarget().GetWorldRepresentation().transform.position - origin).normalized;

                // 1) Perform the raycast
                RaycastHit[] hits = Physics.RaycastAll(origin, direction, Mathf.Infinity, mask);

                // 2) Sort hits by distance so nearest is first
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                // 3) Process sorted hits
                foreach (var hit in hits)
                {
                    GameObject selfGo = GetSelf().GetWorldRepresentation().gameObject;
                    if (hit.collider.gameObject == selfGo ||
                        hit.collider.transform.root == GetSelf().GetWorldRepresentation().transform)
                    {
                        continue;
                    }

                    if (hit.collider.gameObject.TryGetComponent<IEntity>(out IEntity entity) &&
                        entity == GetTarget())
                    {
                        Debug.DrawRay(origin, direction * hit.distance, Color.green);
                        return BtStatus.Success;
                    }

                    Debug.DrawRay(origin, direction * hit.distance, Color.red);
                    // Blocked by another object
                    return BtStatus.Failure;
                }

                // Nothing hit
                return BtStatus.Failure;
            }
        }

        public class LoSCheckNode2D : EnemyNode
        {
            private readonly LayerMask mask;

            public LoSCheckNode2D(BlackBoard bb, LayerMask mask)
                : base(bb)
            {
                this.mask = mask;
            }

            internal override bool HasRequiredMemory()
            {
                return GetSelf() != null && GetTarget() != null;
            }

            public override void Enter()
            {
            }

            public override void Exit()
            {
            }

            public override BtStatus InternalTick()
            {
                if (!HasRequiredMemory())
                    return BtStatus.Failure;

                Vector3 pos = GetSelf().GetWorldRepresentation().transform.position;
                Vector2 dir = (GetTarget().GetWorldRepresentation().transform.position - pos).normalized;
                RaycastHit2D[] hits = Physics2D.RaycastAll(pos, dir, 1000, mask);

                foreach (var hit in hits)
                {
                    GameObject selfGo = GetSelf().GetWorldRepresentation().gameObject;
                    if (hit.collider.gameObject == selfGo ||
                        hit.collider.transform.root == GetSelf().GetWorldRepresentation().transform)
                    {
                        continue;
                    }

                    if (hit.collider.gameObject.TryGetComponent<IEntity>(out IEntity entity) &&
                        entity == GetTarget())
                    {
                        return BtStatus.Success;
                    }

                    // Blocked by another object
                    return BtStatus.Failure;
                }

                // Nothing hit
                return BtStatus.Failure;
            }
        }

        public class FaceNode2D : EnemyNode
        {
            private Vector2 dir;
            public FiniteTimer RotateTimer;
            public bool Invert;
            public bool InvertTransform;
            public bool IsUp;
            public bool Flip;
            public Vector2 Size;
            public string BlackBoardName;
            private AngleClamper clamper;

            public FaceNode2D(BlackBoard bb, FiniteTimer rotateTimer, bool invert, bool invertTransform, bool isUp,
                bool flipTransform, Vector2 initSize, string dataName, AngleClamper clamper = null) : base(bb)
            {
                this.RotateTimer = rotateTimer;
                Invert = invert;
                Flip = flipTransform;
                Size = initSize;
                BlackBoardName = dataName;
                this.clamper = clamper;
                InvertTransform = invertTransform;
            }

            internal override bool HasRequiredMemory()
            {
                return BlackBoard.Get<Transform>(BlackBoardName) != null && GetTarget() != null;
            }

            internal virtual Vector2 GetDir()
            {
                if (!HasRequiredMemory())
                    return Vector2.zero;
                return BlackBoard.Get<Transform>(BlackBoardName).position -
                       GetTarget().GetWorldRepresentation().transform.position;
            }

            public override void Enter()
            {
                RotateTimer.reset();
            }

            public override void Exit()
            {
            }

            public override BtStatus InternalTick()
            {
                if (!HasRequiredMemory())
                    return BtStatus.Failure;
                Transform _body = BlackBoard.Get<Transform>(BlackBoardName);
                bool _cc = RotateTimer.tryCompleteTimer(GetSelf().EntityTicker.DeltaTime);
                float _t = Mathf.Clamp01(RotateTimer.Timer / RotateTimer.MaxValue);

                DoFace(_body, _t, BlackBoard, GetDir(), Invert, InvertTransform, IsUp, Size, Flip, clamper);


                if (_cc)
                {
                    return BtStatus.Success;
                }

                return BtStatus.Running;
            }

            public static void DoFace(Transform body, float t, BlackBoard black, Vector2 dir, bool invert,
                bool invertTransform, bool up, Vector2 initSize, bool flip, AngleClamper clamper = null)
            {
                if (clamper == null)
                    clamper = AngleClamper.FREE;
                dir.Normalize();
                float _m = invert ? 1f : -1f;
                if (black.Has("FacingRight"))
                {
                    UpdateFacing(dir, black);
                    if (black.Get<bool>("FacingRight"))
                        _m *= -1;
                }

                if (up)
                    body.up = Vector3.Lerp(body.up, clamper.Clamp(dir) * _m, t);
                else
                    body.right = Vector3.Lerp(body.right, clamper.Clamp(dir) * _m, t);
                //Debug.DrawRay(body.transform.position,clamper.Clamp(dir) * (_m * 5),Color.blue,0.1f,true);
                if (flip)
                {
                    TryFlipTransform(body, dir, black, initSize, invertTransform ? -1 : 1);
                }
            }

            public static void TryFlipTransform(Transform transform, Vector2 dir, BlackBoard blackBoard, Vector2 size,
                float mInvert)
            {
                var _face = UpdateFacing(dir, blackBoard);
                transform.localScale = new Vector2((_face ? size.x : -size.x) * mInvert, size.y);
            }

            public static bool UpdateFacing(Vector2 dir, BlackBoard blackBoard)
            {
                bool _face = dir.x > 0;
                blackBoard.Set("FacingRight", _face);
                return _face;
            }

            public override string EditorName => $"{GetType().Name} ({RotateTimer.remaingValue()}s left)";
        }

        public class FaceNodeContinuous2D : EnemyNode
        {
            private Vector2 dir;
            public bool Invert;
            public string InvertName;
            public bool IsUp;
            public float Speed;
            public Vector2 Size;
            public bool Flip;
            public string BlackBoardName;
            private AngleClamper clamper;

            public FaceNodeContinuous2D(BlackBoard bb, bool invert, bool invertTransform, bool isUp, float speed,
                bool flipTransform, Vector2 initSize, string dataName, AngleClamper clamper = null) : base(bb)
            {
                if (clamper == null)
                    clamper = AngleClamper.FREE;
                this.clamper = clamper;
                Invert = invert;
                Speed = speed;
                Flip = flipTransform;
                Size = initSize;
                BlackBoardName = dataName;
                InvertTransform = invertTransform;
            }

            public FaceNodeContinuous2D(BlackBoard bb, string Invert_name, bool invertTransform, bool isUp, float speed,
                bool flipTransform, Vector2 initSize, string dataName, AngleClamper clamper = null) : base(bb)
            {
                if (clamper == null)
                    clamper = AngleClamper.FREE;
                this.clamper = clamper;
                //Invert = invert;
                InvertName = Invert_name;
                Speed = speed;
                Flip = flipTransform;
                Size = initSize;
                BlackBoardName = dataName;
                InvertTransform = invertTransform;
            }

            public bool InvertTransform { get; set; }

            internal override bool HasRequiredMemory()
            {
                return BlackBoard.Get<Transform>(BlackBoardName) != null && GetTarget() != null;
            }

            internal virtual Vector2 GetDir()
            {
                if (!HasRequiredMemory())
                    return Vector2.zero;
                return BlackBoard.Get<Transform>(BlackBoardName).position -
                       GetTarget().GetWorldRepresentation().transform.position;
            }

            public override void Enter()
            {
            }

            public override void Exit()
            {
            }

            public override BtStatus InternalTick()
            {
                if (!HasRequiredMemory())
                    return BtStatus.Failure;

                Transform _body = BlackBoard.Get<Transform>(BlackBoardName);
                //Debug.Log(BlackBoard.Get<bool>(InvertName));
                FaceNode2D.DoFace(_body, Time.deltaTime * Speed, BlackBoard, GetDir(),
                    string.IsNullOrEmpty(InvertName) ? Invert : BlackBoard.Get<bool>(InvertName), InvertTransform, IsUp,
                    Size, Flip);

                return BtStatus.Running;
            }
        }

        public class EnemyFacingContinuousNode2D : EnemyNode
        {
            public FaceDirNodeContinuous2D FaceDir;
            public FaceNodeContinuous2D Face;
            public LoSCheckNode2D LoS;

            public EnemyFacingContinuousNode2D(BlackBoard bb, bool invert, bool invertTransform, bool isUp, float speed,
                bool flipTransform, Vector2 initSize, string dataName, string dirDataName, LayerMask losMask) : base(bb)
            {
                Face = new FaceNodeContinuous2D(bb, invert, invertTransform, isUp, speed, flipTransform, initSize,
                    dataName);
                FaceDir = new FaceDirNodeContinuous2D(bb, invert, !invertTransform, isUp, speed / 2, flipTransform,
                    initSize,
                    dataName, dirDataName);
                LoS = new LoSCheckNode2D(bb, losMask);
            }

            public override void Enter()
            {
                LoS.Enter();
                Face.Enter();
                FaceDir.Enter();
            }

            public override void Exit()
            {
                LoS.Exit();
                Face.Exit();
                FaceDir.Exit();
            }

            public override BtStatus InternalTick()
            {
                if (LoS.Tick() == BtStatus.Success)
                    Face.Tick();
                else
                    FaceDir.Tick();
                return BtStatus.Running;
            }

            internal override bool HasRequiredMemory()
            {
                return LoS.HasRequiredMemory() && Face.HasRequiredMemory() && FaceDir.HasRequiredMemory();
            }
        }

        public class EnemyWaitNode : EnemyNode
        {
            public EnemyWaitNode(BlackBoard bb, float dur) : base(bb)
            {
                Timer.resetAndSetMax(dur);
            }

            protected FiniteTimer Timer;


            public override void Enter()
            {
                Timer.reset();
            }

            public override BtStatus InternalTick()
            {
                if (Timer.tryCompleteTimer(GetSelf().EntityTicker.DeltaTime))
                {
                    return BtStatus.Success;
                }

                return BtStatus.Running;
            }

            public override void Exit()
            {
                Timer.reset();
            }

            internal override bool HasRequiredMemory()
            {
                return GetSelf() != null;
            }
        }

        public class FaceDirNode2D : FaceNode2D
        {
            readonly string data_dirname;

            public FaceDirNode2D(BlackBoard bb, FiniteTimer rotateTimer, bool invert, bool invertTransform, bool isUp,
                bool flipTransform, Vector2 initSize, string dataName, string dataDirname,
                AngleClamper clamper = null) : base(bb, rotateTimer, invert, invertTransform, isUp, flipTransform,
                initSize,
                dataName, clamper)
            {
                this.data_dirname = dataDirname;
            }

            internal override Vector2 GetDir()
            {
                return BlackBoard.Has(data_dirname) ? BlackBoard.Get<Vector2>(data_dirname) : base.GetDir();
            }
        }

        public class FaceDirNodeContinuous2D : FaceNodeContinuous2D
        {
            public string DataDirname;

            public FaceDirNodeContinuous2D(BlackBoard bb, bool invert, bool invertTransform, bool isUp, float speed,
                bool flipTransform, Vector2 initSize, string dataName, string dataDirName,
                AngleClamper clamper = null) : base(bb, invert, invertTransform, isUp, speed, flipTransform, initSize,
                dataName, clamper)
            {
                DataDirname = dataDirName;
            }

            internal override Vector2 GetDir()
            {
                return BlackBoard.Has(DataDirname) ? BlackBoard.Get<Vector2>(DataDirname) : base.GetDir();
            }
        }


        public class PlayAnimationNode : EnemyNode
        {
            readonly Animator animator;
            readonly string name;
            readonly int cache;
            readonly float crossfadeDur;
            readonly PaNodeMode playmode;

            public enum PaNodeMode
            {
                AnPlay,
                AnCrossFade,
                AnPlayNoRestart
            }

            public PlayAnimationNode(BlackBoard bb, Animator animator, string name, float crossfadeDur = 0,
                PaNodeMode mode = PaNodeMode.AnPlay) : base(bb)
            {
                this.animator = animator;
                this.name = name;
                this.crossfadeDur = crossfadeDur;
                playmode = mode;
            }

            public PlayAnimationNode(BlackBoard bb, Animator animator, int cachedName, float crossfadeDur = 0,
                PaNodeMode mode = PaNodeMode.AnPlay) : base(bb)
            {
                this.animator = animator;
                cache = cachedName;
                this.crossfadeDur = crossfadeDur;
                name = "";
                playmode = mode;
            }

            internal override bool HasRequiredMemory()
            {
                return animator != null;
            }

            public override void Enter()
            {
            }

            public override BtStatus InternalTick()
            {
                if (HasRequiredMemory())
                {
                    switch (playmode)
                    {
                        case PaNodeMode.AnPlay:
                            SafePlay(animator, name, cache);
                            break;
                        case PaNodeMode.AnCrossFade:
                            SafeCrossFade(animator, name, cache, crossfadeDur);
                            break;
                        case PaNodeMode.AnPlayNoRestart:
                            SafePlayNoRestart(animator, name, cache);
                            break;
                        default:
                            SafePlay(animator, name, cache);
                            break;
                    }

                    return BtStatus.Success;
                }

                return BtStatus.Failure;
            }

            private void SafePlay(Animator animator, string name, int cachedHash)
            {
                if (!string.IsNullOrEmpty(name))
                    animator.Play(name);
                else
                    animator.Play(cachedHash);
            }

            private void SafePlayNoRestart(Animator animator, string name, int cachedHash)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);


                if (!string.IsNullOrEmpty(name))
                {
                    if (!state.IsName(name))
                    {
                        animator.Play(name);
                    }
                }
                else
                {
                    if (state.shortNameHash != cachedHash)
                    {
                        animator.Play(cachedHash);
                    }
                }
            }

            private void SafeCrossFade(Animator animator, string name, int cachedHash, float dur)
            {
                AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

                if (crossfadeDur == 0)
                    throw new Exception("YO! YOU FORGOT TO ADD THE CROSSFADE DURATION YOU DUMB ASS!");

                if (!string.IsNullOrEmpty(name))
                {
                    if (!state.IsName(name))
                        animator.CrossFade(name, dur);
                    else
                        SafePlay(animator, name, cachedHash);
                }
                else
                {
                    if (state.shortNameHash != cachedHash)
                        animator.CrossFade(cachedHash, dur);
                    else
                        SafePlay(animator, name, cachedHash);
                }
            }

            public override void Exit()
            {
            }
        }
    }
}