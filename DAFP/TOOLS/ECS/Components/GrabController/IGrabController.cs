using System;
using DAFP.TOOLS.ECS.Components.GrabController;
using UnityEngine;

namespace DAFP.TOOLS.ECS.Components.GrabController
{
    public interface IGrabController
    {
        public void Simulate();
        public void Attach(GameObject obj);
        public void Detach();
        public float DefaultHoldDistance { set; }
        public float MassWhileCarring { set; }
        public float Damping { set; }
        public float MaxExitVelocity { set; }
        public Rigidbody AttachedBody { get; }
        public Transform Cam { set; }
        public event Action<Rigidbody> OnAttached;
        public event Action<Rigidbody> OnDeattached;
    }
}