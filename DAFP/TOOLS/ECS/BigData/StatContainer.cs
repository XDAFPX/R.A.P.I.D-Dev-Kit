using System;
using System.Collections.Generic;
using System.Linq;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.Utill;
using TNRD;
using UnityEngine;
using Zenject;

namespace DAFP.TOOLS.ECS.BigData
{
    [Serializable]
    [CreateAssetMenu(fileName = "Stats", menuName = "R.A.P.I.D/StatContainer")]
    public class StatContainer : ScriptableObject, ITickable, IStatContainer
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

        public IStatContainer MarkAsDirty()
        {
            return InvalidateCache();
        }

        public IStatContainer InvalidateCache()
        {
            isDirty = true;
            return this;
        }

        public IStatContainer Construct(IEntity parent)
        {
            foreach (var _statBase in get_nodes())
            {
                _statBase.ChangeOwner(parent);
            }

            foreach (var _statBase in Stats.ToValues())
            {
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
            IStatBase _statBase = null;
            var _pathBuilder = new StatInjector.PathBuilder(name);

            IStatBase _current = Stats.ToValues().FindByName(_pathBuilder.GetRoot());
            if (_current == null)
                throw_if_stat_invalid();

            // If only root exists, we found it and also it should be the type expected
            _statBase = _current;
            if (_pathBuilder.GetTotalDepth() == 0 && type_check(out var _stat1)) return _stat1;


            // Traverse down the tree, skipping the root (already found)
            foreach (var _segment in _pathBuilder.IterateRootToLeaf().Skip(1))
            {
                _current = _current.Pets.Cast<IStatBase>().FindByName(_segment);
                if (_current == null)
                    throw_if_stat_invalid();
            }

            _statBase = _current;
            if (type_check(out var _stat)) return _stat;

            throw_if_stat_invalid();
            return null;
            
            
            
            void throw_if_stat_invalid()
            {
                throw new Exception(
                    $"The stat you had searched for is either an incorrect type ({typeof(T).Name}) or null ");
            }


            bool type_check(out IStat<T> stat)
            {
                if (_statBase is IStat<T> _found)
                {
                    stat = _found;
                    return true;
                }

                stat = null;
                return false;
            }
        }

        public bool Has(string statName, out IStatBase stat)
        {
            stat = null;
            if (Has(new StatInjector.PathBuilder(statName), out var _base))
                stat = _base;
            return stat != null;
        }


        public bool Has(StatInjector.PathBuilder pathBuilder)
        {
            return Has(pathBuilder, out var _base);
        }

        public bool Has(StatInjector.PathBuilder pathBuilder, out IStatBase statBase)
        {
            statBase = null;
            if (pathBuilder == null)
                return false;

            IStatBase _current = Stats.ToValues().FindByName(pathBuilder.GetRoot());
            if (_current == null)
                return false;

            // If only root exists, we found it
            statBase = _current;
            if (pathBuilder.GetTotalDepth() == 0)
                return true;

            // Traverse down the tree, skipping the root (already found)
            foreach (var _segment in pathBuilder.IterateRootToLeaf().Skip(1))
            {
                _current = _current.Pets.Cast<IStatBase>().FindByName(_segment);
                if (_current == null)
                    return false;
            }

            statBase = _current;
            return true;
        }

        public bool Has(string statName)
        {
            return Has(new StatInjector.PathBuilder(statName));
        }

        public IStatContainer Add(IStatBase stat)
        {
            Stats.Add(new SerializableInterface<IStatBase>(stat));
            InvalidateCache();
            return this;
        }

        public bool Add(StatInjector.PathBuilder pathBuilder, IStatBase statToAdd)
        {
            if (pathBuilder == null || statToAdd == null)
                return false;
            if (pathBuilder.GetTotalDepth() == 0) //failsafe if passed a normal stat
            {
                Add(statToAdd);
                return true;
            }

            IStatBase _current = Stats.ToValues().FindByName(pathBuilder.GetRoot());
            if (_current == null)
                return false;
            if (pathBuilder.GetTotalDepth() == 1)
            {
                ((IOwnerOf<IStatBase>)_current).AddPet(statToAdd);
                InvalidateCache();
                return true;
            }

            // Traverse down the tree to find the parent, skipping the root (already found)
            var data = pathBuilder.IterateRootToLeaf().ToArray();
            // We already have the root in _current; start from the first child (index 1)
            // and stop at the parent of the leaf (last index - 1)
            for (int i = 1; i < data.Length - 1; i++)
            {
                _current = _current.Pets.Cast<IStatBase>().FindByName(data[i]);
                if (_current == null)
                    return false;
            }

            // Add the stat to the found parent's children
            ((IOwnerOf<IStatBase>)_current).AddPet(statToAdd);
            InvalidateCache();
            return true;
        }

        public void Tick()
        {
            var nodes = get_nodes().OfType<ITickable>();
            foreach (var _statBase in nodes)
            {
                _statBase.Tick();
            }
        }
    }
}