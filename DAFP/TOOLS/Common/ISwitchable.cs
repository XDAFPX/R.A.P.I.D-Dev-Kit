namespace DAFP.TOOLS.Common
{
    public interface ISwitchable
    {
        bool Enabled { get; }
        void Enable();
        void Disable();
    }
}