using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.ECS;

namespace RapidLib.DAFP.TOOLS.Common
{
    public  class EntityPetImpl  : IEntityPet
    {
        
        private readonly List<IEntity> _owners = new List<IEntity>();
        private IEntity _currentOwner;
        private IEntity _previousOwner;
        private bool _enabled;

        // List of all entities that have ever owned this view
        public List<IEntity> Owners => _owners;

        // Returns the current owner
        public IEntity GetCurrentOwner() => _currentOwner;

        // Returns the previous owner before the last ChangeOwner call
        public IEntity GetExOwner() => _previousOwner;

        // Switches to a new owner, storing the old one in _previousOwner
        public void ChangeOwner(IEntity newOwner)
        {
            if (_currentOwner != null)
            {
                _previousOwner = _currentOwner;
            }

            _currentOwner = newOwner;

            if (newOwner != null && !_owners.Contains(newOwner))
            {
                _owners.Add(newOwner);
            }
        }
    }
}