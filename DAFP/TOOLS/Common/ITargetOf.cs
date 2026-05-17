using Optional;
using Optional.Unsafe;

namespace RapidLib.DAFP.TOOLS.Common
{
    public interface ITargetOf<T>
    {
        public Option<T> Value { get; set; }

        public bool HasValue => Value.HasValue;
        public T Raw => Value.ValueOrFailure("uhhh");
    }

    public class TargetOf<T> : ITargetOf<T>
    {
        public TargetOf(T value)
        {
            Value = value.Some();
        }

        public Option<T> Value { get; set; }
    }
}