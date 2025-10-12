using System;
using DAFP.TOOLS.Common;

namespace DAFP.TOOLS.ECS.DebugSystem
{
    public class DebugDrawLayer : ISwitchable, INameable
    {
        public DebugDrawLayer(string name,bool enabled =false)
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

    }
}