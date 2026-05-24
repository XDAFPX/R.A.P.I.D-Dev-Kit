using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS.BigData;

namespace DAFP.TOOLS.ECS
{
    public interface IHaveStats : IOwnerOf<IStatBase>
    {
        public IStatContainer Stats { get; }
    }
}