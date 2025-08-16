using System;
using DAFP.TOOLS.ECS.BigData.Health;

namespace DAFP.TOOLS.ECS.BigData.GlobalModifiers
{
    public class GlobalSpeedBoard : GlobalFloatMultiplyBoard<SpeedModdable>
    {
        // Inherit all behavior from GlobalFloatModifierBoard<SpeedModdable>
        // Override here if you need to customize for speed specifically
    }

    public class SpeedModdable : Attribute
    {
    }
}