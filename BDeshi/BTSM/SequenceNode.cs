using System.Collections.Generic;
using UnityEngine;

namespace BDeshi.BTSM
{
    /// <summary>
    /// Continue running one by one until one fail, then it itself fails.
    /// </summary>
    public class SequenceNode : BtMultiDecorator
    {
        [SerializeField] protected List<IBtNode> Children;
        [SerializeField] private int curIndex;
        public override IEnumerable<IBtNode> GetActiveChildren => Children;
        public override void AddChild(IBtNode child)
        {
            Children.Add(child);
        }

        public SequenceNode(List<IBtNode> children)
        {
            this.Children = children;
        }
        public SequenceNode()
        {
            this.Children = new List<IBtNode>();
        }

        public override void Enter()
        {
            curIndex = 0;
            if (curIndex >= Children.Count || curIndex < 0)
            {
                return;
            }
            else
            {
                Children[curIndex].Enter();
            }
        }


        public override BtStatus InternalTick()
        {
            if (curIndex >= Children.Count || curIndex < 0)
            {
                
                return BtStatus.Success;
            }
            else
            {
                var _childResult = Children[curIndex].Tick();
                // Debug.Log(curIndex + " "  + childResult);

                if (_childResult == BtStatus.Failure)
                    return BtStatus.Failure;
                else if (_childResult == BtStatus.Success)
                {
                    Children[curIndex].Exit();

                    curIndex++;
                    if (curIndex <= (Children.Count - 1))
                    {
                        Children[curIndex].Enter();
                    }
                }
            }

            return BtStatus.Running;
        }

        public override void Exit()
        {
            if (curIndex < Children.Count  && curIndex >= 0)
            {
                Children[curIndex].Exit();
            }
        }

    }
}