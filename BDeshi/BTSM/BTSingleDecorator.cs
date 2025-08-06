using System.Collections.Generic;

namespace BDeshi.BTSM
{
    public abstract class BtSingleDecorator : BtDecorator
    {
        
        protected IBtNode Child;
        public override  IEnumerable<IBtNode> GetActiveChildren => GetChildWrapper();

        IEnumerable<IBtNode> GetChildWrapper()
        {
            yield return Child;
        }

        protected BtSingleDecorator(IBtNode child)
        {
            this.Child = child;
        }
    }
}