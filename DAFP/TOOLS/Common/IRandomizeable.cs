using NRandom;

namespace DAFP.TOOLS.Common
{
    public interface IRandomizeable
    {
        public void Randomize(IRandom rng, float margin01);
    }
}