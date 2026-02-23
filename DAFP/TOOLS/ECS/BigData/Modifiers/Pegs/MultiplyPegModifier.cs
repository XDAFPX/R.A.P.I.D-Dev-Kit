using System;

namespace DAFP.TOOLS.ECS.BigData.Modifiers.Pegs
{
    [System.Serializable]
    public class MultiplyPegModifier : PegModifier, IPegStatModifier<int>, IPegStatModifier<uint>,
        IPegStatModifier<float>
    {
        public int Apply(int value)
        {
            return DefaultMultiply(value, Peg.GetAbsoluteValue());
        }

        public uint Apply(uint value)
        {
            return DefaultMultiply(value, Peg.GetAbsoluteValue());
        }

        public float Apply(float value)
        {
            return DefaultMultiply(value, Peg.GetAbsoluteValue());
        }


        public static T DefaultMultiply<T>(T a, object b)
        {
            var cc = StatInjector.try_convert(b, typeof(T));
            
            if (cc == null)
                return a;

            if (a is int ai && cc is int bi)
                return (T)(object)(ai * bi);

            if (a is uint au && cc is uint bu)
                return (T)(object)(au * bu);

            if (a is float af && cc is float bf)
                return (T)(object)(af * bf);

            return a;
        }
    }
}