using System;

namespace DAFP.TOOLS.ECS.BigData.GlobalModifiers
{
    public class GlobalHealthBoard : GlobalFloatMultiplyBoard<HealthModdable>
    {
    }

    public class HealthModdable : Attribute
    {
    }
}