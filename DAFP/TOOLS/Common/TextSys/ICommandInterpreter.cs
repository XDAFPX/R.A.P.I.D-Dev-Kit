using RapidLib.DAFP.TOOLS.Common;

namespace DAFP.TOOLS.Common.TextSys
{
    public interface ICommandInterpreter : IPetOwnerTreeOf<ICommandInterpreter> 
    {
        public string Procces(string input);
    }
}