using System.Collections.Generic;

namespace DAFP.TOOLS.Common
{
    public interface IPet<TOwner> : IOwnable<TOwner> where TOwner : IOwner<TOwner>
    {
        List<TOwner> Owners { get; }

        TOwner GetExOwner()
        {
            return Owners.Count >= 2 ? Owners[^2] : default;
        }

        TOwner IOwnable<TOwner>.GetCurrentOwner()
        {
            return Owners.Count > 0 ? Owners[^1] : default;
        }

        void IOwnable<TOwner>.ChangeOwner(TOwner newOwner)
        {
            var current = GetCurrentOwner();
            if (current == null)
            {
                newOwner?.AddPet(this);
                if (newOwner != null) Owners.Add(newOwner);
            }
            else
            {
                current.RemovePet(this);
                if (newOwner != null)
                {
                    Owners.Add(newOwner);
                    newOwner.AddPet(this);
                }
                else
                {
                    // If newOwner is null, just clear ownership without adding
                }
            }
        }
    }
}