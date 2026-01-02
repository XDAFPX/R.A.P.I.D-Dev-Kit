using DAFP.TOOLS.Common.Maths;

namespace DAFP.TOOLS.ECS.Components
{
    // Common interface extracted from UniversalMoverBase. Vector parameters are unified to IVectorBase.
    public interface IUniversalMover
    {
        // Capabilities/state
        bool CanFly { get; set; }
        bool IsInKnockback { get; }
        bool IsInDash { get; }
        float MaxFallSpeed { get; set; }

        // Inputs / abilities (vector parameters expressed via IVectorBase)
        void Input(IVectorBase input);
        void DoDash(IVectorBase force, float time);
        void DoJump(IVectorBase jump);
        void DoHalt(float divisor);
        void DoCutJump(IVectorBase positive, float m);
        void AddKnockback(IVectorBase force, float time, float delay);
    }
}
