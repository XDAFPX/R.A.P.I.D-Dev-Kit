using UnityEngine;

namespace PixelRouge.Inspector
{
    /// <summary>
    /// Change the field name showed on the inspector.
    /// </summary>
    public sealed class LabelAttribute : PropertyAttribute
    {
        public string OverriddenLabel { get; set; }

        public LabelAttribute(string newLabel)
        {
            OverriddenLabel = newLabel;
        }
    }
}