namespace DAFP.TOOLS.Common.TextSys
{
    public interface ICommandInterpriter : IOwner<ICommandInterpriter>, IPet<ICommandInterpriter>
    {
        public string Procces(string input);
    }
}