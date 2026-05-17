using DAFP.TOOLS.Common.Maths;

namespace DAFP.TOOLS.Common
{
    public interface IMover
    {
        bool CanFly { get; set; }
        bool IsInKnockback { get; }
        bool IsInDash { get; }

        void Input(IVector input);
        void DoDash(IVector force, float time);
        void DoJump(IVector jump);
        void DoHalt(float divisor);
        void DoCutJump(IVector positive, float m);
        void AddKnockback(IVector force, float time, float delay);

        void AddForceNormal(IVector force);
        void AddForceImpulse(IVector force);
    }
}