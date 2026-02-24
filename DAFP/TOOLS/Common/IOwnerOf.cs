using System.Collections.Generic;
using System.Linq;

namespace DAFP.TOOLS.Common
{
    public interface IOwnerBase
    {
        IEnumerable<object> AbsolutePets => Enumerable.Empty<object>();
    }

    public interface IOwnerOf<TPet> : IOwnerBase 
    {
        IEnumerable<TPet> Pets { get; }

        void AddPet(TPet pet);

        bool RemovePet(TPet pet);

        IEnumerable<object> IOwnerBase.AbsolutePets => Pets.OfType<object>();
    }
}