using System;
using System.Collections.Generic;
using Bdeshi.Helpers.Utility;
using UnityEngine;
namespace BDeshi.BTSM
{
    public abstract class BtMultiDecorator: BtDecorator
    {
        public abstract void AddChild(IBtNode child);

        public BtMultiDecorator AppendChild(IBtNode child)
        {
            AddChild(child);
            return this;
        }
    }

    public abstract class BtDecorator : BtNodeBase
    {
        public abstract IEnumerable<IBtNode> GetActiveChildren { get; }
    }
    public class MonitorNode : BtMultiDecorator
    {
        private Func<bool> condition;
        private IBtNode child;

        public MonitorNode(Func<bool> condition)
        {
            this.condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }

        public override void AddChild(IBtNode child)
        {
            this.child = child ?? throw new ArgumentNullException(nameof(child));
        }

        public override IEnumerable<IBtNode> GetActiveChildren
        {
            get
            {
                if (condition != null && condition())
                    yield return child;
            }
        }

        public override void Enter()
        {
            if (condition())
                child?.Enter();
        }

        public override void Exit()
        {
            child?.Exit();
        }

        public override BtStatus InternalTick()
        {
            if (!condition())
                return BtStatus.Failure;

            if (child == null)
                return BtStatus.Failure;

            return child.Tick();
        }
    }

    public class SelectorNode : BtMultiDecorator
    {
        private readonly List<IBtNode> children = new List<IBtNode>();
        private int currentIndex = 0;

        public SelectorNode(List<IBtNode> children)
        {
            this.children = children;
        }

        public override void AddChild(IBtNode child)
        {
            if (child != null)
                children.Add(child);
        }

        public override IEnumerable<IBtNode> GetActiveChildren => children;

        public override void Enter()
        {
            currentIndex = 0;
            if (children.Count > 0)
                children[currentIndex].Enter();
        }

        public override BtStatus InternalTick()
        {
            while (currentIndex < children.Count)
            {
                IBtNode _currentNode = children[currentIndex];
                BtStatus _status = _currentNode.Tick();

                if (_status == BtStatus.Running)
                    return BtStatus.Running;
                if (_status == BtStatus.Success)
                    return BtStatus.Success;

                _currentNode.Exit();
                currentIndex++;

                if (currentIndex < children.Count)
                    children[currentIndex].Enter();
            }

            return BtStatus.Failure;
        }

        public override void Exit()
        {
            if (currentIndex < children.Count)
                children[currentIndex].Exit();
        }
    }

    public class ConditionNode:BtNodeBase
    {
        private Func<bool> func;
        public BtStatus SuccessState;
        public BtStatus FailState;

        public ConditionNode(Func<bool> func, BtStatus successState = BtStatus.Success, BtStatus failState = BtStatus.Failure)
        {
            this.func = func;
            this.SuccessState = successState;
            this.FailState = failState;
        }

        public override void Enter( )
        {
            
        }

        public override void Exit()
        {
            
        }

        public override BtStatus InternalTick()
        {
            return func() ? SuccessState : FailState;
        }
    }
    
    public class MaintainConditionNode:BtNodeBase
    {
        private Func<bool> func;
        public BtStatus SuccessState;
        public BtStatus WaitState;
        public bool ResetOnFail;
        public FiniteTimer MaintainTimer;

        public MaintainConditionNode(Func<bool> func, float maintainTime, bool resetOnFail = true, BtStatus successState = BtStatus.Success, BtStatus waitState = BtStatus.Running)
        {
            this.func = func;
            this.SuccessState = successState;
            this.WaitState = waitState;
            this.MaintainTimer = new FiniteTimer(maintainTime);
            this.ResetOnFail = resetOnFail;
        }

        public override void Enter( )
        {
            MaintainTimer.reset();
        }

        public override void Exit()
        {
            
        }

        public override BtStatus InternalTick()
        {
            bool _success = func();
            if (_success)
            {
                if (MaintainTimer.tryCompleteTimer(Time.deltaTime))
                {
                    return SuccessState;
                }
                else
                {
                    return BtStatus.Running;
                }
            }
            else
            {
                if (ResetOnFail)
                {
                    MaintainTimer.reset();
                    
                }

                return WaitState;
            }
        }
    }

    public class TimerNode:BtSingleDecorator
    {
        public FiniteTimer Duration;
        public BtStatus TimeoutStatus = BtStatus.Success;

    
        public override string EditorName => $"{base.EditorName} [{Duration.remaingValue()}] left";
        public override void Enter()
        {
            Duration.reset();
            Child.Enter();   
        }

        public override void Exit()
        {
            Child.Exit();
        }

        public override BtStatus InternalTick()
        {
            Duration.updateTimer(Time.deltaTime);
            if (Duration.isComplete)
            {
                return TimeoutStatus;
            }
            else
            {

    
                return Child.Tick();
            }
        }

        public TimerNode(IBtNode child, float timeDuration) : base(child)
        {
            this.Duration = new FiniteTimer(timeDuration);
        }

    }

    public interface IBtNode
    {
        void Enter();
        
        BtStatus Tick();
        void Exit();

        public BtStatus LastStatus { get; }

        public string Prefix { get; set; } 
        public string Typename => GetType().Name;
        public  string EditorName => Prefix +"_"+ Typename;

    }

    public abstract class BtNodeBase : IBtNode
    {
        public abstract void Enter();

        /// <summary>
        /// NOT VIRTUAL OR ABSTRACT. DO NOT OVERRIDE.
        /// Override internal tick instead.
        /// This just calls InternalTick() and saves the result onto lastStatus
        /// </summary>
        /// <returns></returns>
        public BtStatus Tick()
        {
            lastStatus = InternalTick();
            return lastStatus;
        }

        /// <summary>
        /// Called everytime the node is ticked
        /// Override this for subclass nodes
        /// </summary>
        /// <returns>The result from ticking </returns>
        public abstract BtStatus InternalTick();
        /// <summary>
        /// Result from last tick
        /// </summary>
        public BtStatus LastStatus => lastStatus;
        public abstract void Exit();
        public string Prefix { get; set; }

        protected BtStatus lastStatus = BtStatus.NotRunYet;
        public string Typename => GetType().Name;
        public virtual string EditorName => Prefix +"_"+ Typename;
    }
    
    public abstract class BtNodeMonoBase : MonoBehaviour,IBtNode
    {
        public abstract void Enter();

        /// <summary>
        /// NOT VIRTUAL OR ABSTRACT. DO NOT OVERRIDE.
        /// Override internal tick instead.
        /// This approach is for making tracking status is easier
        /// </summary>
        /// <returns></returns>
        public BtStatus Tick()
        {
            lastStatus = InternalTick();
            return lastStatus;
        }

        /// <summary>
        /// To allow caching status onto lastStatus
        /// </summary>
        /// <returns></returns>
        public abstract BtStatus InternalTick();

        public BtStatus LastStatus => lastStatus;
        public abstract void Exit();
        public string Prefix { get; set; }

        protected BtStatus lastStatus = BtStatus.NotRunYet;
        public string Typename => GetType().Name;
        public virtual string EditorName => Prefix +"_"+ Typename;
    }

    public enum BtStatus{
        NotRunYet,
        /// <summary>
        /// Is actively running, will block sequence nodes
        /// </summary>
        Running,
        Success,
        Failure,
        /// <summary>
        /// Neither success nor failure, non blocking running
        /// Ex Use case: Parallel node where you want to keep running child regardless of what others do 
        /// </summary>
        Ignore
        

    }
}
