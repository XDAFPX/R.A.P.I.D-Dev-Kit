using System.Collections.Generic;

namespace DAFP.TOOLS.Common
{
    public interface IOwner<TOwner> where TOwner : IOwner<TOwner>
    {
        ISet<IOwnable<TOwner>> Pets { get; }

        void AddPet(IOwnable<TOwner> pet)
        {
            if (pet == this)
                return;
            if (pet == null)
                return;
            Pets.Add(pet);
        }

        bool RemovePet(IOwnable<TOwner> pet)
        {
            if (pet == this)
                return false;
            if (pet == null)
                return false;
            Pets.Remove(pet);
            return true;
        }
    }
}