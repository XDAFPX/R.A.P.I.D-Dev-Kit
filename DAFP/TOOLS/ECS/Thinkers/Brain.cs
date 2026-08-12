using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BDeshi.BTSM;
using DAFP.TOOLS.Common;
using DAFP.TOOLS.Common.TextSys;
using DAFP.TOOLS.Common.Utill;
using DAFP.TOOLS.ECS.DebugSystem;
using DAFP.TOOLS.ECS.Serialization;
using DAFP.TOOLS.ECS.Services;
using ModestTree;
using RapidLib.DAFP.TOOLS.Common;
using TNRD;
using UGizmo;
using UnityEngine;
using UnityGetComponentCache;
using Zenject;

namespace DAFP.TOOLS.ECS.Thinkers
{
    public abstract class Brain : ScriptableObject, IThinker, IThinkerLogic
    {
        // -- Dependencies
        [Inject] public IDebugSys<IGlobalGizmos, IConsoleMessenger> DebugSystem { get; }

        [Inject] protected DiContainer Injector;
        [Inject] protected World World;

        [Inject] protected ISaveSystem SaveSystem;
        // -- Fields
#if UNITY_EDITOR
        [field: SerializeField] public bool EditMode { get; set; }
#endif

        [SerializeField] private ChildrenResolveStrategy Strategy;

        // NOTE: Unity serializes enums by their underlying int value, not by name.
        // The first two entries keep their original order/values (0, 1) so existing
        // serialized data on prefabs/assets is preserved. New entries are appended.
        private enum ChildrenResolveStrategy
        {
            /// <summary>Children run first, then this brain. Applied identically on Start, Tick, and End.</summary>
            FirstTickChildren = 0,

            /// <summary>This brain runs first, then children. Applied identically on Start, Tick, and End.</summary>
            FirstTickMe = 1,

            /// <summary>Only children run; this brain's own Start/Tick/End logic is skipped entirely.</summary>
            ChildrenOnly = 2,

            /// <summary>Only this brain runs; children are not started/ticked/ended by this brain.</summary>
            MeOnly = 3,

            /// <summary>Only brain runs, until throw children are not ticked by this brain.</summary>
            RunChildrenOnlyWhenIThrow = 4
        }

        [SerializeField] private List<SerializableInterface<IThinker>> ChildThinkers;

        private List<IDebugDrawable> debugDrawOwners = new();
        protected List<IThinker> ParentThinkers = new();

        public bool HasWoken { get; set; }
        /*[field: SerializeField]*/

        // -- Core Methods


        void IThinkerLogic.Start(IEntity host)
        {
            AnimationNameCacheInitializer.InitializeCaches(this);

            InternalStart(host);
            initialize_children(host);

            init_debug_drawers(SetupDebugDrawers(host));
        }

        void IThinkerLogic.Tick(IEntity host, ITickerBase ticker)
        {
            run_tick(host, ticker, Strategy);
        }

        void IThinkerLogic.End(IEntity host)
        {
            InternalEnd(host);
            end_children(host);
        }

        /// <summary>
        /// Single source of truth for ordering. Used by Start, Tick, and End so that
        /// whatever ordering rule the designer picked applies identically at every
        /// lifecycle stage (no drift between how a brain wakes up and how it tears down).
        /// </summary>
        private void run_tick(IEntity host, ITickerBase ticker, ChildrenResolveStrategy strat)
        {
            switch (strat)
            {
                case ChildrenResolveStrategy.FirstTickChildren:
                    tick_children(host, ticker);
                    InternalTick(host, ticker);
                    break;

                case ChildrenResolveStrategy.FirstTickMe:
                    InternalTick(host, ticker);
                    tick_children(host, ticker);
                    break;

                case ChildrenResolveStrategy.ChildrenOnly:
                    tick_children(host, ticker);
                    break;

                case ChildrenResolveStrategy.MeOnly:
                    InternalTick(host, ticker);
                    break;

                case ChildrenResolveStrategy.RunChildrenOnlyWhenIThrow:

                    try
                    {
                        InternalTick(host, ticker);
                    }
                    catch (Exception e)
                    {
                        tick_children(host, ticker);
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(Strategy), Strategy, "Unhandled ChildrenResolveStrategy");
            }
        }


        protected abstract void InternalStart(IEntity host);
        protected abstract void InternalTick(IEntity host, ITickerBase ticker);
        protected abstract void InternalEnd(IEntity host);
        protected abstract IEnumerable<IDebugDrawer> SetupDebugDrawers(IEntity host);

        private void de_init_debug_drawers() //--move //TODO
        {
            if (debugDrawPets == null || debugDrawPets.IsEmpty())
                return;
            debugDrawPets.Clear();
            DebugSystem.RemovePet(this);
        }

        private void init_debug_drawers(IEnumerable<IDebugDrawer> pets)
        {
            if (pets == null)
                return;
            var _debugDrawers = pets as IDebugDrawer[] ?? pets.ToArray();
            if (_debugDrawers.IsEmpty())
                return;
            debugDrawPets = debugDrawPets.Union<IDebugDrawable>(_debugDrawers).ToList();
            var petsSnapshot = debugDrawPets.ToArray();

            foreach (var _ownable in petsSnapshot)
            {
                _ownable.ChangeOwner(this);
                if (_ownable is IDebugDrawer _drawer)
                    _drawer.InitilizeDebugDrawer(DebugSystem);
            }

            DebugSystem.AddPet(this);
        }

        private void tick_children(IEntity host, ITickerBase ticker)
        {
            foreach (var _child in ChildThinkers)
            {
                if (_child.Value is IThinkerLogic _l)
                    _l.Tick(host, ticker);
            }
        }


        private void end_children(IEntity host) //bad vibe
        {
            foreach (var _child in ChildThinkers)
            {
                if (_child.Value is IThinkerLogic _l)
                    _l.End(host);
            }
        }

        private void initialize_children(IEntity host)
        {
            foreach (var _child in ChildThinkers)
            {
                (_child.Value).ChangeOwner((IThinker)this);
                if (_child.Value is IThinkerLogic _l)
                    _l.Start(host);
            }
        }


        List<IDebugDrawable> IPetOf<IDebugDrawable, IDebugDrawable>.Owners => debugDrawOwners;

        private List<IDebugDrawable> debugDrawPets = new();
        public List<IThinker> Children => ((IOwnerOf<IThinker>)this).Pets.ToList();

        IEnumerable<IThinker> IOwnerOf<IThinker>.Pets => ChildThinkers.ToValues();

        public void AddPet(IThinker pet)
        {
            if (pet == null) return;
            if (ChildThinkers.ToValues().Contains(pet)) return;
            ChildThinkers.Add(new SerializableInterface<IThinker>(pet));
        }

        public bool RemovePet(IThinker pet)
        {
            if (pet == null) return false;
            if (!ChildThinkers.ToValues().Contains(pet)) return false;
            ChildThinkers.Remove(new SerializableInterface<IThinker>(pet));
            return true;
        }

        public IEnumerable<IDebugDrawable> Pets => debugDrawPets;

        List<IThinker> IPetOwnerTreeOf<IThinker>.Owners => ParentThinkers;

        IEnumerable<object> IOwnerBase.AbsolutePets => AbsolutePets();


        protected virtual IEnumerable<object> AbsolutePets()
        {
            return debugDrawPets.Union(ChildThinkers.ToValues());
        }

        public void AddPet(IDebugDrawable pet)
        {
            if (pet == null) return;
            if (Pets.Contains(pet)) return;
            debugDrawPets.Add(pet);
        }

        public bool RemovePet(IDebugDrawable pet)
        {
            if (pet == null) return false;
            if (!Pets.Contains(pet)) return false;
            debugDrawPets.Remove(pet);
            return true;
        }

        public string Name
        {
            get => name;
            set => name = value;
        }
    }
}