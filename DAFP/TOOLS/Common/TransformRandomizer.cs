using DAFP.TOOLS.Common.Utill;
using UnityEngine;
using NRandom;

namespace RapidLib.DAFP.TOOLS.Common
{
    /// <summary>
    /// Randomizes a Transform's position, rotation, and/or scale within configurable ranges.
    /// Scale randomization can be absolute (pick a new value) or proportional
    /// (multiply the current scale by a random factor, preserving relative axis ratios).
    /// </summary>
    [DisallowMultipleComponent]
    public class TransformRandomizer : MonoBehaviour, IRandomizer
    {
        public enum ScaleMode
        {
            Absolute,      // pick each axis independently within min/max
            Proportional,  // multiply current scale by one random factor (uniform)
            ProportionalPerAxis // multiply each current axis by its own random factor
        }

        [Header("Position")]
        [SerializeField] private bool randomizePosition = false;
        [SerializeField] private bool localPosition = true;
        [SerializeField] private Vector3 positionMin = Vector3.zero;
        [SerializeField] private Vector3 positionMax = Vector3.zero;

        [Header("Rotation")]
        [SerializeField] private bool randomizeRotation = false;
        [SerializeField] private bool localRotation = true;
        [SerializeField] private Vector3 rotationEulerMin = Vector3.zero;
        [SerializeField] private Vector3 rotationEulerMax = new Vector3(0, 360, 0);

        [Header("Scale")]
        [SerializeField] private bool randomizeScale = false;
        [SerializeField] private ScaleMode scaleMode = ScaleMode.Proportional;

        [Tooltip("Used when Scale Mode = Absolute. Picks each axis independently.")]
        [SerializeField] private Vector3 scaleMin = Vector3.one * 0.8f;
        [SerializeField] private Vector3 scaleMax = Vector3.one * 1.2f;

        [Tooltip("Used when Scale Mode = Proportional / ProportionalPerAxis. " +
                 "e.g. 0.8 to 1.2 means \u00b120% of current scale.")]
        [SerializeField] private float proportionalScaleMin = 0.8f;
        [SerializeField] private float proportionalScaleMax = 1.2f;

        public void Randomize(IRandom rng)
        {
            if (rng == null)
            {
                Debug.LogWarning($"[{nameof(TransformRandomizer)}] Randomize called with null rng on {name}.", this);
                return;
            }

            if (randomizePosition) RandomizePosition(rng);
            if (randomizeRotation) RandomizeRotation(rng);
            if (randomizeScale) RandomizeScale(rng, scaleMode);
        }

        // ---------------- Position ----------------

        public void RandomizePosition(IRandom rng)
        {
            Vector3 pos = new Vector3(
                rng.Range(positionMin.x, positionMax.x),
                rng.Range(positionMin.y, positionMax.y),
                rng.Range(positionMin.z, positionMax.z));

            if (localPosition) transform.localPosition = pos;
            else transform.position = pos;
        }

        // ---------------- Rotation ----------------

        public void RandomizeRotation(IRandom rng)
        {
            Vector3 euler = new Vector3(
                rng.Range(rotationEulerMin.x, rotationEulerMax.x),
                rng.Range(rotationEulerMin.y, rotationEulerMax.y),
                rng.Range(rotationEulerMin.z, rotationEulerMax.z));

            Quaternion rot = Quaternion.Euler(euler);

            if (localRotation) transform.localRotation = rot;
            else transform.rotation = rot;
        }

        // ---------------- Scale ----------------

        public void RandomizeScale(IRandom rng, ScaleMode mode)
        {
            switch (mode)
            {
                case ScaleMode.Absolute:
                    RandomizeScaleAbsolute(rng);
                    break;
                case ScaleMode.Proportional:
                    RandomizeScaleProportional(rng);
                    break;
                case ScaleMode.ProportionalPerAxis:
                    RandomizeScaleProportionalPerAxis(rng);
                    break;
            }
        }

        /// <summary>Picks an entirely new scale, each axis independent of current value.</summary>
        public void RandomizeScaleAbsolute(IRandom rng)
        {
            transform.localScale = new Vector3(
                rng.Range(scaleMin.x, scaleMax.x),
                rng.Range(scaleMin.y, scaleMax.y),
                rng.Range(scaleMin.z, scaleMax.z));
        }

        /// <summary>
        /// Multiplies the CURRENT scale by a single random factor, uniformly across
        /// all axes — preserves the object's current axis ratios exactly.
        /// </summary>
        public void RandomizeScaleProportional(IRandom rng)
        {
            float factor = rng.Range(proportionalScaleMin, proportionalScaleMax);
            transform.localScale = transform.localScale * factor;
        }

        /// <summary>
        /// Multiplies the CURRENT scale by an independent random factor per axis.
        /// Ratios are not preserved, but each axis still scales relative to itself
        /// rather than jumping to an absolute value.
        /// </summary>
        public void RandomizeScaleProportionalPerAxis(IRandom rng)
        {
            Vector3 current = transform.localScale;
            transform.localScale = new Vector3(
                current.x * rng.Range(proportionalScaleMin, proportionalScaleMax),
                current.y * rng.Range(proportionalScaleMin, proportionalScaleMax),
                current.z * rng.Range(proportionalScaleMin, proportionalScaleMax));
        }

        /// <summary>
        /// Utility: multiplies scale by a factor within a custom range, ignoring the
        /// serialized proportionalScaleMin/Max fields. Useful for one-off calls from code
        /// (e.g. "shrink this by 10-30%" without touching inspector settings).
        /// </summary>
        public void RandomizeScaleProportional(IRandom rng, float min, float max)
        {
            float factor = rng.Range(min, max);
            transform.localScale = transform.localScale * factor;
        }
    }
}
