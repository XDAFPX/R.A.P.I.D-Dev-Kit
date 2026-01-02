using System;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public class DebugDrawLayer : ISwitchable, INameable
    {
        public DebugDrawLayer(string name, bool enabled = false)
        {
            Name = name;
            Enabled = enabled;
        }

        public bool Enabled { set; get; }

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }

        public string Name { get; set; }

        public static class DefaultDebugLayers
        {
            public const string TRIGGERS = "Triggers";
            public const string HIT_BOXES = "HitBoxes";
            public const string BOUNDING_BOXES = "BoundingBoxes";
            public const string POSITIONS = "Positions";
            public const string EYE_VECTORS = "EyeVectors";
            public const string VELOCITIES = "Velocities";
            public const string NAMES = "Names";
            public const string VIEW_MODELS = "ViewModels";
            public const string THINKERS = "Thinkers";
        }
    }
}