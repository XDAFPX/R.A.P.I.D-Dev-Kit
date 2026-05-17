using System;
using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.Environment;
using DAFP.TOOLS.ECS.Services;

namespace RapidLib.DAFP.TOOLS.Common
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;

    [DisallowMultipleComponent]
    public class UniversalCollisionManager : EntityComponent
    {
        [Header("Settings")] [SerializeField] private LayerMask LayersToIgnore;
        [SerializeField] private List<GameObject> SpecificObjectsToIgnore = new List<GameObject>();

        private struct ColliderObjState
        {
            public Component Component;
            public Action<bool> SetEnabled;
            public bool WasEnabled;
        }

        private List<ColliderObjState> capturedStates = new List<ColliderObjState>();
        private bool isDisabled = false;


        protected override void OnTick()
        {
        }

        protected override void OnInitialize()
        {
        }

        public override ITickerBase EntityComponentTicker => World.EMPTY_TICKER;

        public void SetCollisionState(bool active)
        {
            if (active) restore_collisions();
            else capture_and_disable();
        }

        private void capture_and_disable()
        {
            if (isDisabled) return;
            capturedStates.Clear();

            var _colliders3D = GetComponentsInChildren<Collider>(true);
            var _colliders2D = GetComponentsInChildren<Collider2D>(true);
            var coliders = _colliders2D.Select((collider2D1 => new UniversalCollider(collider2D1)))
                .Concat(_colliders3D.Select((collider1 => new UniversalCollider(collider1))));
            foreach (var _col in coliders)
            {
                // Перевірка на ігнорування
                if (should_ignore(_col.gameObject)) continue;

                capturedStates.Add(new ColliderObjState
                {
                    Component = _col.component,
                    WasEnabled = _col.enabled,
                    SetEnabled = (b => _col.enabled = b)
                });

                _col.enabled = false;
            }

            isDisabled = true;
        }

        private void restore_collisions()
        {
            if (!isDisabled) return;

            foreach (var _state in capturedStates.Where(state => state.Component != null))
            {
                _state.SetEnabled.Invoke(_state.WasEnabled);
            }

            capturedStates.Clear();
            isDisabled = false;
        }

        private bool should_ignore(GameObject go)
        {
            // Перевірка по LayerMask
            if (((1 << go.layer) & LayersToIgnore) != 0) return true;

            // Перевірка по списку об'єктів
            if (SpecificObjectsToIgnore.Contains(go)) return true;

            return false;
        }

        public static UniversalCollisionManager EnsureOn(GameObject target)
        {
            var _manager = target.GetComponent<UniversalCollisionManager>();
            if (_manager == null) _manager = target.AddComponent<UniversalCollisionManager>();
            return _manager;
        }
    }
}