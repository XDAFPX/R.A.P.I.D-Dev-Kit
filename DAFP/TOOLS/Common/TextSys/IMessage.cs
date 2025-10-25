namespace DAFP.TOOLS.Common.TextSys
{
    public interface IMessage
    {
        public string Print();

        public static IMessage Literal(string txt) => CompText.Literal(txt);
    }
}