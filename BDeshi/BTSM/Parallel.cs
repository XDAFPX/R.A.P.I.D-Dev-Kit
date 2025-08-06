using System.Collections.Generic;
using Bdeshi.Helpers.Utility.Extensions;

namespace BDeshi.BTSM
{
    /// <summary>
    /// Run all children every tick
    /// Succeed on first on succeeding, or when all do.
    /// If the later, the children may or may not restart
    /// </summary>
    public class Parallel: BtMultiDecorator
    {
        public override IEnumerable<IBtNode> GetActiveChildren => children;
        private List<IBtNode> children;
        private int runThreshold;
        private bool allMustSucceed;
        /// <summary>
        /// Will it restart children that have succeded if allMustSucceed = true?
        /// </summary>
        private bool repeatSuccessfullChildren;
        

        public Parallel(List<IBtNode> activeChildren, bool allMustSucceed =false, bool repeatSuccessfullChildren =false)
        {
            this.children = activeChildren;
            this.allMustSucceed = allMustSucceed;
            this.repeatSuccessfullChildren = repeatSuccessfullChildren;
        }
        
        public Parallel(bool allMustSucceed =false, bool repeatSuccessfullChildren = false)
            : this(new List<IBtNode>(), allMustSucceed, repeatSuccessfullChildren)
        {
    
        }


        public override void Enter()
        {
            runThreshold = children.Count;

            foreach (var _btNodeBase in children)
            {
                _btNodeBase.Enter();
            }
        }

        public override BtStatus InternalTick()
        {

            bool _anySuccess = false;
            bool _allSuccess = true;
            for(int _i = runThreshold - 1; _i >= 0; _i--)
            {
                var _btNodeBase = children[_i];
                var _status = _btNodeBase.Tick();
                if (_status== BtStatus.Success)
                {
                    _anySuccess = true;
                    if (allMustSucceed)
                    {
                        if (repeatSuccessfullChildren)
                        {
                            children[_i].Exit();
                            children[_i].Enter();
                        }else
                        {
                            // Debug.Log(children[i].Typename + " complete " + i + " " + runThreshold + children[i].LastStatus);

                            children.swapToLast(_i);
                            runThreshold--;

                            // Debug.Log(children[children.Count -1 ].Typename + "afterwards complete " + (children.Count - 1) + " " + runThreshold + children[children.Count -1 ].LastStatus);
                            
                        }
                    }

                }
                else if(_status != BtStatus.Ignore)
                {
                    _allSuccess = false;
                }
            }

            // foreach (var child in children)
            // {
            //     Debug.Log(child.Typename + " " + child.LastStatus);
            // }

            if ((allMustSucceed && _allSuccess) || (!allMustSucceed && _anySuccess))
                return BtStatus.Success;
            return BtStatus.Running;
        }


        public override void Exit()
        {
            foreach (var _btNodeBase in children)
            {
                _btNodeBase.Exit();
            }
        }

        public override void AddChild(IBtNode child)
        {
            children.Add(child);
        }
    }
    
    /// <summary>
    /// Run all children every tick
    /// Succeed on first on succeeding, or when all do.
    /// If the later, the children may or may not restart
    /// </summary>
    public class ParallelRepeat: BtMultiDecorator
    {
        public override IEnumerable<IBtNode> GetActiveChildren => children;
        private List<IBtNode> children;
        private int runThreshold;
        public BtStatus RunninStatus = BtStatus.Running;
        

        public ParallelRepeat(List<IBtNode> activeChildren)
        {
            this.children = activeChildren;
        }
        

        public override void Enter()
        {
            runThreshold = children.Count;

            foreach (var _btNodeBase in children)
            {
                _btNodeBase.Enter();
            }
        }

        public override BtStatus InternalTick()
        {
            for(int _i = 0; _i < children.Count; _i++)
            {
                var _child = children[_i];
                var _status = _child.Tick();
                if (_status== BtStatus.Success || _status == BtStatus.Failure)
                {
                    _child.Exit();
                    _child.Enter();
                }
            }

            return BtStatus.Running;
        }


        public override void Exit()
        {
            foreach (var _btNodeBase in children)
            {
                _btNodeBase.Exit();
            }
        }

        public override void AddChild(IBtNode child)
        {
            children.Add(child);
        }
    }
}