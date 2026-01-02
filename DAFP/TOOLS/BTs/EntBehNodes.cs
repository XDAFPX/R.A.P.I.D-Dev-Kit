using System;
using BDeshi.BTSM;
using Bdeshi.Helpers.Utility;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Maths;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BigData;
using DAFP.TOOLS.ECS.Components;
using UnityEngine;

namespace DAFP.TOOLS.BTs
{
    public class EntBehNodes
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
            private readonly Action action;
            private readonly Action @out;

            public ActionNode(Action action,Action Out)
            {
                Out = default;
                this.action = action;
                @out = Out;
            }

            public ActionNode(Action action)
            {
                this.action = action;
            }

            public override void Enter()
            {
            }

            public override void Exit()
            {
                @out?.Invoke();
                
            }

            public override BtStatus InternalTick()
            {
                action.Invoke();
                return BtStatus.Success;
            }
        }

        public class HaltNode : EntNode
        {
            private readonly UniversalMover2D mover2D;
            private readonly UniversalMover3D mover3D;
            private readonly IUniversalMover imover;
            private readonly IStat<float> divisor;

            public HaltNode(IEntity ent, IStat<float> divisor) : base(ent.Memory)
            {
                this.divisor = divisor;
                if (ent.GetWorldRepresentation().TryGetComponent(out IUniversalMover mm))
                {
                    imover = mm;
                }
            }

            public HaltNode(BlackBoard bb, IUniversalMover imover, IStat<float> divisor) : base(bb)
            {
                this.imover = imover;
                this.divisor = divisor;
            }

            public HaltNode(BlackBoard bb, UniversalMover3D mover3D, IStat<float> divisor) : base(bb)
            {
                this.mover3D = mover3D;
                this.divisor = divisor;
            }

            public HaltNode(BlackBoard bb, UniversalMover2D mover2D, IStat<float> divisor) : base(bb)
            {
                this.mover2D = mover2D;
                this.divisor = divisor;
            }

            public override void Enter()
            {
            }

            public override void Exit()
            {
            }

            public override BtStatus InternalTick()
            {
                mover2D?.DoHalt(divisor.Value);
                mover3D?.DoHalt(divisor.Value);
                imover?.DoHalt(divisor.Value);
                return BtStatus.Success;
            }

            internal override bool HasRequiredMemory()
            {
                return true;
            }
        }

        public class DashNode : EntNode
        {
            private readonly UniversalMover2D movement2d;
            private readonly UniversalMover3D movement;
            private readonly IUniversalMover i_mover;
            private readonly string dashDirName;
            private readonly IStat<IVectorBase> dir;
            private readonly IStat<float> dur;
            private readonly EntWaitNode wait;

            public DashNode(IEntity ent, IStat<IVectorBase> dashDir, IStat<float> dur) : base(ent.Memory)
            {
                if (ent.GetWorldRepresentation().TryGetComponent(out IUniversalMover _mover))
                    i_mover = _mover;
                dir = dashDir;
                this.dur = dur;
                wait = new EntWaitNode(ent.Memory, dur.Value);
            }

            public DashNode(IEntity ent, string dashDirName, IStat<float> dur) : base(ent.Memory)
            {
                if (ent.GetWorldRepresentation().TryGetComponent(out IUniversalMover _mover))
                    i_mover = _mover;
                this.dashDirName = dashDirName;
                this.dur = dur;
                wait = new EntWaitNode(ent.Memory, dur.Value);
            }

            public DashNode(BlackBoard bb, IUniversalMover movement, string dashDirName, IStat<float> dur) : base(bb)
            {
                i_mover = movement;
                this.dashDirName = dashDirName;
                this.dur = dur;
                wait = new EntWaitNode(bb, dur.Value);
            }

            public DashNode(BlackBoard bb, UniversalMover3D movement, string dashDirName, IStat<float> dur) : base(bb)
            {
                this.movement = movement;
                this.dashDirName = dashDirName;
                this.dur = dur;
                wait = new EntWaitNode(bb, dur.Value);
            }

            public DashNode(BlackBoard bb, UniversalMover2D movement2d, string dashDirName, IStat<float> dur) : base(bb)
            {
                this.movement2d = movement2d;
                this.dashDirName = dashDirName;

                this.dur = dur;
                wait = new EntWaitNode(bb, dur.Value);
            }

            public override void Enter()
            {
                movement?.DoDash(BlackBoard.Get<Vector3>(dashDirName), dur.Value);

                movement2d?.DoDash(BlackBoard.Get<Vector2>(dashDirName), dur.Value);
                i_mover.DoDash(BlackBoard.Get<IVectorBase>(dashDirName) ?? dir.Value, dur.Value);
                wait.Enter();
            }

            public override void Exit()
            {
            }

            public override BtStatus InternalTick()
            {
                if (!HasRequiredMemory())
                    return BtStatus.Failure;
                return wait.Tick();
            }

            internal override bool HasRequiredMemory()
            {
                return BlackBoard.Has(dashDirName) || dir != default;
            }
        }

        public class LoSCheckNode3D : EntNode
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

                var origin = GetSelf().GetWorldRepresentation().transform.position;
                var direction = (GetTarget().GetWorldRepresentation().transform.position - origin).normalized;

                // 1) Perform the raycast
                var hits = Physics.RaycastAll(origin, direction, Mathf.Infinity, mask);

                // 2) Sort hits by distance so nearest is first
                Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

                // 3) Process sorted hits
                foreach (var hit in hits)
                {
                    var selfGo = GetSelf().GetWorldRepresentation().gameObject;
                    if (hit.collider.gameObject == selfGo ||
                        hit.collider.transform.root == GetSelf().GetWorldRepresentation().transform)
                        continue;

                    if (hit.collider.gameObject.TryGetComponent<IEntity>(out var entity) &&
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

        public class LoSCheckNode2D : EntNode
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

                var pos = GetSelf().GetWorldRepresentation().transform.position;
                Vector2 dir = (GetTarget().GetWorldRepresentation().transform.position - pos).normalized;
                var hits = Physics2D.RaycastAll(pos, dir, 1000, mask);

                foreach (var hit in hits)
                {
                    var selfGo = GetSelf().GetWorldRepresentation().gameObject;
                    if (hit.collider.gameObject == selfGo ||
                        hit.collider.transform.root == GetSelf().GetWorldRepresentation().transform)
                        continue;

                    if (hit.collider.gameObject.TryGetComponent<IEntity>(out var entity) &&
                        entity == GetTarget())
                        return BtStatus.Success;

                    // Blocked by another object
                    return BtStatus.Failure;
                }

                // Nothing hit
                return BtStatus.Failure;
            }
        }

        public class FaceNode2D : EntNode
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
                RotateTimer = rotateTimer;
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
                var _body = BlackBoard.Get<Transform>(BlackBoardName);
                var _cc = RotateTimer.tryCompleteTimer(GetSelf().EntityTicker.DeltaTime);
                var _t = Mathf.Clamp01(RotateTimer.Timer / RotateTimer.MaxValue);

                DoFace(_body, _t, BlackBoard, GetDir(), Invert, InvertTransform, IsUp, Size, Flip, clamper);


                if (_cc) return BtStatus.Success;

                return BtStatus.Running;
            }

            public static void DoFace(Transform body, float t, BlackBoard black, Vector2 dir, bool invert,
                bool invertTransform, bool up, Vector2 initSize, bool flip, AngleClamper clamper = null)
            {
                if (clamper == null)
                    clamper = AngleClamper.FREE;
                dir.Normalize();
                var _m = invert ? 1f : -1f;
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
                if (flip) TryFlipTransform(body, dir, black, initSize, invertTransform ? -1 : 1);
            }

            public static void TryFlipTransform(Transform transform, Vector2 dir, BlackBoard blackBoard, Vector2 size,
                float mInvert)
            {
                var _face = UpdateFacing(dir, blackBoard);
                transform.localScale = new Vector2((_face ? size.x : -size.x) * mInvert, size.y);
            }

            public static bool UpdateFacing(Vector2 dir, BlackBoard blackBoard)
            {
                var _face = dir.x > 0;
                blackBoard.Set("FacingRight", _face);
                return _face;
            }

            public override string EditorName => $"{GetType().Name} ({RotateTimer.remaingValue()}s left)";
        }

        public class FaceNodeContinuous2D : EntNode
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

                var _body = BlackBoard.Get<Transform>(BlackBoardName);
                //Debug.Log(BlackBoard.Get<bool>(InvertName));
                FaceNode2D.DoFace(_body, Time.deltaTime * Speed, BlackBoard, GetDir(),
                    string.IsNullOrEmpty(InvertName) ? Invert : BlackBoard.Get<bool>(InvertName), InvertTransform, IsUp,
                    Size, Flip);

                return BtStatus.Running;
            }
        }

        public class EntFacingContinuousNode2D : EntNode
        {
            public FaceDirNodeContinuous2D FaceDir;
            public FaceNodeContinuous2D Face;
            public LoSCheckNode2D LoS;

            public EntFacingContinuousNode2D(BlackBoard bb, bool invert, bool invertTransform, bool isUp, float speed,
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

        public class EntWaitNode : EntNode
        {
            public EntWaitNode(BlackBoard bb, float dur) : base(bb)
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
                if (Timer.tryCompleteTimer(GetSelf().EntityTicker.DeltaTime)) return BtStatus.Success;

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
            private readonly string data_dirname;

            public FaceDirNode2D(BlackBoard bb, FiniteTimer rotateTimer, bool invert, bool invertTransform, bool isUp,
                bool flipTransform, Vector2 initSize, string dataName, string dataDirname,
                AngleClamper clamper = null) : base(bb, rotateTimer, invert, invertTransform, isUp, flipTransform,
                initSize,
                dataName, clamper)
            {
                data_dirname = dataDirname;
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


        public class PlayAnimationNode : EntNode
        {
            private readonly Animator animator;
            private readonly string name;
            private readonly int cache;
            private readonly float crossfadeDur;
            private readonly PaNodeMode playmode;

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
                var state = animator.GetCurrentAnimatorStateInfo(0);


                if (!string.IsNullOrEmpty(name))
                {
                    if (!state.IsName(name)) animator.Play(name);
                }
                else
                {
                    if (state.shortNameHash != cachedHash) animator.Play(cachedHash);
                }
            }

            private void SafeCrossFade(Animator animator, string name, int cachedHash, float dur)
            {
                var state = animator.GetCurrentAnimatorStateInfo(0);

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

        public class EntCooldownNode : BtSingleDecorator
        {
            private readonly Cooldown cooldown;

            public EntCooldownNode(IBtNode child, Cooldown cooldown) : base(child)
            {
                this.cooldown = cooldown;
            }

            public override void Enter()
            {
                Child.Enter();
            }

            public override void Exit()
            {
                Child.Enter();
            }

            public override BtStatus InternalTick()
            {
                if (cooldown.IsOnCooldown)
                {
                    return BtStatus.Failure;
                }
                else
                {
                    var s = Child.Tick();
                    if (s == BtStatus.Failure || s == BtStatus.Success)
                    {
                        cooldown.ResetToDefault();
                    }

                    return s;
                }
            }
        }

        public class WaitAndDoNode : BtSingleDecorator
        {
            private readonly ITickerBase ticker;

            public WaitAndDoNode(IBtNode child, ITickerBase ticker) : base(child)
            {
                this.ticker = ticker;
            }

            public override void Enter()
            {
                Child.Enter();
            }

            public override void Exit()
            {
                Child.Exit();
            }

            private FiniteTimer timer = new();

            public override BtStatus InternalTick()
            {
                if (!timer.tryCompleteTimer(ticker.DeltaTime))
                {
                    Child.Tick();

                    return BtStatus.Running;
                }

                return BtStatus.Success;
            }
        }
    }
}