using System;
using Zenject;

namespace DAFP.TOOLS.ECS.Thinkers
{
    public class BrainTicker : ITickable
    {
        private readonly IEntity ent;
        private readonly ITickerBase tickerBase;

        public BrainTicker(IEntity ent, ITickerBase tickerBase)
        {
            this.ent = ent;
            this.tickerBase = tickerBase;
        }

        public void Tick()
        {
            if(ent.Brains is IThinkerLogic _logic)
                _logic.Tick(ent,tickerBase);
        }
    }
}