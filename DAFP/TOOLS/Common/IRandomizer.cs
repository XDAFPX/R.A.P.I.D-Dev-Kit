using NRandom;

namespace RapidLib.DAFP.TOOLS.Common
{
    public interface IRandomizer
    {
        public void Randomize(IRandom rng);
    }

    // public interface IRandomizer<in T> : IRandomizer
    // {
    //     public void Randomize(T obj);
    // }
}