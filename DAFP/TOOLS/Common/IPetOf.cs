using System.Collections.Generic;

namespace DAFP.TOOLS.Common
{
    public interface IPetOf<TOwner,TSelf> : IOwnedBy<TOwner> where TOwner : IOwnerOf<TSelf> where TSelf : IOwnedBy<TOwner>
    {
        List<TOwner> Owners { get; }

        TOwner GetExOwner()
        {
            return Owners.Count >= 2 ? Owners[^2] : default;
        }

        TOwner IOwnedBy<TOwner>.GetCurrentOwner()
        {
            return Owners.Count > 0 ? Owners[^1] : default;
        }

        void IOwnedBy<TOwner>.ChangeOwner(TOwner newOwner)
        {
            var _current = GetCurrentOwner();
            if (_current == null)
            {
                newOwner?.AddPet((TSelf)this);
                if (newOwner != null) Owners.Add(newOwner);
            }
            else
            {
                _current.RemovePet((TSelf)this);
                if (newOwner != null)
                {
                    Owners.Add(newOwner);
                    newOwner.AddPet((TSelf)this);
                }
                else
                {
                    // If newOwner is null, just clear ownership without adding
                }
            }
        }
    }
}