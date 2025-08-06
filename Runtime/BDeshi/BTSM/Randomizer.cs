using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace BDeshi.BTSM
{
    public class Randomizer : BtMultiDecorator
    {
        private List<IBtNode> children = new List<IBtNode>();
        private List<float> weights = new List<float>();
        private List<Func<bool>> availabilityFunc;
        private int pickIndex = 0;
        
        // #TODO add random weight readjustment when a child is picked
        

        public int Pick()
        {
            float _randomVal = Random.Range(0, CalcTotalWeight());
            float _runningSum = 0;
            for (pickIndex = 0; pickIndex < children.Count && (weights[pickIndex] + _runningSum)  < _randomVal; pickIndex++)
            {
                _runningSum += weights[pickIndex];
            }

            return pickIndex;
        }

        public float CalcTotalWeight()
        {
            float _sum = 0;
            foreach (var _weight in weights)
            {
                _sum += _weight;
            }

            return _sum;
        }

        public override void Enter()
        {
            Pick();
            children[pickIndex].Enter();
        }

        public override BtStatus InternalTick()
        {
            return children[pickIndex].Tick();
        }

        public override void Exit()
        {
            children[pickIndex].Tick();
        }

        public override IEnumerable<IBtNode> GetActiveChildren => children;
        public override void AddChild(IBtNode child)
        {
            children.Add(child);
            weights.Add(1);
        }
        
        public virtual Randomizer AppendChild(float weight, IBtNode child)
        {
            children.Add(child);
            weights.Add(weight);
            return this;
        }
    }
}