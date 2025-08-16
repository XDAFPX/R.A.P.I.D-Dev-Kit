namespace Archon.SwissArmyLib.ResourceSystem
{
    public interface IPercentageProvider
    {
        float Percentage { get; }
        string GetFormatedMaxValue();
        string GetFormatedCurValue();
    }
}