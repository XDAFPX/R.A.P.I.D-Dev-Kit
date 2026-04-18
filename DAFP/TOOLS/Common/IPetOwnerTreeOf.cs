using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;

namespace RapidLib.DAFP.TOOLS.Common
{
    public interface IPetOwnerTreeOf<TNode> : IOwnerOf<TNode>, IOwnedBy<TNode>
        where TNode : IOwnerOf<TNode>, IOwnedBy<TNode>
    {
        List<TNode> Children { get; }
    
        IEnumerable<TNode> IOwnerOf<TNode>.Pets => Children;

        void IOwnerOf<TNode>.AddPet(TNode node)
        {
            if (node == null) return;
            if (Pets.Contains(node)) return;
            if (Children == null) throw new Exception("(IPetOwnerTree) You forgot to declare Children {get; }!!");
            Children.Add(node);
        }

        bool IOwnerOf<TNode>.RemovePet(TNode node)
        {
            if (node == null) return false;
            if (!Pets.Contains(node)) return false;
            if (Children == null) throw new Exception("(IPetOwnerTree) You forgot to declare Children {get; }!!");
            Children.Remove(node);
            return true;
        }

        List<TNode> Owners { get; }

        TNode GetExOwner()
        {
            return Owners.Count >= 2 ? Owners[^2] : default;
        }

        TNode IOwnedBy<TNode>.GetCurrentOwner()
        {
            if (Owners==null) throw new Exception($"You forgot to initialize the Owners<{typeof(TNode).Name}> list in {GetType()}");
            return Owners.Count > 0 ? Owners[^1] : default;
        }

        void IOwnedBy<TNode>.ChangeOwner(TNode newOwner)
        {
            var _current = GetCurrentOwner();
            if (_current == null)
            {
                newOwner?.AddPet((TNode)this);
                if (newOwner != null) Owners.Add(newOwner);
            }
            else
            {
                _current.RemovePet((TNode)this);
                if (newOwner != null)
                {
                    Owners.Add(newOwner);
                    newOwner.AddPet((TNode)this);
                }
                else
                {
                    // If newOwner is null, just clear ownership without adding
                }
            }
        }
    }
}