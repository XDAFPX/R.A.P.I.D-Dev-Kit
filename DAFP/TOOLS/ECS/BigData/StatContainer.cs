using System;
using System.Collections.Generic;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using TNRD;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BigData
{
    [Serializable]
    [CreateAssetMenu(fileName = "Stats", menuName = "R.A.P.I.D/StatContainer")]
    public class StatContainer : ScriptableObject, ITickable
    {
        [SerializeField] private List<SerializableInterface<IStatBase>> Stats;

        private IEnumerable<IStatBase> cachedNodes;
        private bool isDirty = true;

        private IEnumerable<IStatBase> get_nodes()
        {
            if (isDirty || cachedNodes == null)
            {
                cachedNodes = Stats.ToValues().GetAllNodes();
                isDirty = false;
            }

            return cachedNodes;
        }

        public StatContainer MarkAsDirty()
        {
            return InvalidateCache();
        }

        public StatContainer InvalidateCache()
        {
            isDirty = true;
            return this;
        }

        public StatContainer Construct(IEntity parent)
        {
            foreach (var _statBase in get_nodes())
            {
                if (_statBase is IOwnable<IEntity> _pet)
                    _pet.ChangeOwner(parent);
                if (_statBase is IInitializable _tickable)
                {
                    _tickable.Initialize();
                }
            }

            return this;
        }

        public IStat<T> Get<T>(string name, Func<IStat<T>> fallback)
        {
            try
            {
                return get<T>(name);
            }
            catch (Exception _e)
            {
                return fallback.Invoke();
            }
        }

        private IStat<T> get<T>(string name)
        {
            var _stat = get_nodes().FindByName(name);
            if (_stat is IStat<T> _found)
                return _found;
            throw new Exception(
                $"The stat you had searched for is either an incorrect type ({typeof(T).Name}) or null ");
        }

        public bool Has(string name, out IStatBase stat)
        {
            stat = (IStatBase)get_nodes().FindByName(name);
            return stat != null;
        }

        public bool Has(string name)
        {
            var _stat = get_nodes().FindByName(name);
            return _stat != null;
        }

        public StatContainer Add(IStatBase stat)
        {
            Stats.Add(new SerializableInterface<IStatBase>(stat));
            InvalidateCache();
            return this;
        }

        public void Tick()
        {
            foreach (var _statBase in get_nodes())
            {
                if (_statBase is ITickable _tickable)
                    _tickable.Tick();
            }
        }
    }
}