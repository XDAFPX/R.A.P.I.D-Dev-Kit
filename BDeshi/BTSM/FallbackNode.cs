using System.Collections.Generic;
using UnityEngine;

namespace BDeshi.BTSM
{
    /// <summary>
    /// keep on trying child    ren one by one until one succeeds
    /// </summary>
    public class FallbackNode : BtMultiDecorator
    {
        [SerializeField] List<IBtNode> children;
        [SerializeField] private int curIndex;
        public override IEnumerable<IBtNode> GetActiveChildren => children;
        public bool Retry = true;
        public override void AddChild(IBtNode child)
        {
            children.Add(child);
        }

        public FallbackNode(List<IBtNode> children)
        {
            this.children = children;
        }

        public FallbackNode(bool retry = true)
        {
            this.children = new List<IBtNode>();
            this.Retry = retry;
        }

        public override void Enter()
        {
            curIndex = 0;

            if (curIndex >= children.Count || curIndex < 0)
            {
                return;
            }
            else
            {
                children[curIndex].Enter();
            }
        }


        public override BtStatus InternalTick()
        {

            if (curIndex >= children.Count || curIndex < 0)
            {
                return BtStatus.Failure;
            }
            else
            {
                var _childResult = children[curIndex].Tick();

                if (_childResult == BtStatus.Success)
                    return BtStatus.Success;
                else if (_childResult == BtStatus.Failure)
                {
                    children[curIndex].Exit();
                    
                    curIndex++;
                    if (curIndex <= (children.Count - 1))
                    {
                        children[curIndex].Enter();
                    }
                }
            }

            return BtStatus.Running;
        }

        public override void Exit()
        {
            if (curIndex < children.Count  && curIndex >= 0)
            {
                children[curIndex].Exit();
            }
        }

    }

    public class FallbackRepeatNode : BtMultiDecorator
    {
        [SerializeField] List<IBtNode> children;
        [SerializeField] private int curIndex;
        public override IEnumerable<IBtNode> GetActiveChildren => children;
        public override void AddChild(IBtNode child)
        {
            children.Add(child);
        }

        public FallbackRepeatNode(List<IBtNode> children)
        {
            this.children = children;
        }

        public FallbackRepeatNode()
        {
            this.children = new List<IBtNode>();
        }

        public override void Enter()
        {
            curIndex = 0;

            if (curIndex >= children.Count || curIndex < 0)
            {
                return;
            }
            else
            {
                children[curIndex].Enter();
            }
        }


        public override BtStatus InternalTick()
        {
            for (int _i = 0; _i < curIndex && _i < children.Count; _i++)
            {
                var _child = children[_i];
                _child.Enter();
                var _childResult = _child.Tick();


                if (_childResult != BtStatus.Failure)
                {
                    if (curIndex < children.Count)
                    {
                        children[curIndex].Exit();
                    }
                    curIndex = _i;
                    if (_childResult == BtStatus.Success)
                    {
                        return BtStatus.Success;
                    }
                    return BtStatus.Running;
                }
            }

            for (int _i = curIndex; _i < children.Count; _i++)
            {
                var _child = children[_i];
                if (_i != curIndex)
                {
                    _child.Enter();
                }
                
                curIndex = _i;
                var _childResult = _child.Tick();
                
                if (_childResult == BtStatus.Success)
                {
                    return BtStatus.Success;
                }else if (_childResult == BtStatus.Failure)
                {
                    _child.Exit();
                }
                else
                {
                    return BtStatus.Running;
                }

            }
            //every child failed try from start again next tick()
            curIndex = 0;
            return BtStatus.Running;

        }

        public override void Exit()
        {
            if (curIndex < children.Count  && curIndex >= 0)
            {
                children[curIndex].Exit();
            }
        }
    }
    
    /// <summary>
    /// keep on trying earlier children while running lowermost
    /// Earlier in list = higher priority
    /// </summary>
    public class PriorityFallbackNode : BtMultiDecorator
    {
        [SerializeField] List<IBtNode> children;
        [SerializeField] List<bool> hasRun = new List<bool>();
        [SerializeField] private int tryUpTo;
        public override IEnumerable<IBtNode> GetActiveChildren => children;
        public override void AddChild(IBtNode child)
        {
            children.Add(child);
            hasRun.Add(false);
        }

        public PriorityFallbackNode(List<IBtNode> children)
        {
            this.children = children;
        }

        public PriorityFallbackNode()
        {
            this.children = new List<IBtNode>();
        }

        public override void Enter()
        {
            tryUpTo = children.Count - 1;
        }


        public override BtStatus InternalTick()
        {
            for (int _i = 0; _i < children.Count; _i++)
            {
                if(!hasRun[_i])
                {
                    hasRun[_i] = true;
                    children[_i].Enter();
                }
                
                var _childResult =children[_i].Tick();
                if (_childResult == BtStatus.Success)
                    return BtStatus.Success;
                if (_childResult == BtStatus.Running)
                    break;
            }
            return BtStatus.Running;
        }

        public override void Exit()
        {

            
            for (int _i = 0; _i < children.Count; _i++)
            {
                if(hasRun[_i])
                {
                    hasRun[_i] = false;
                    children[_i].Exit();
                }
            }
        }

    }
}