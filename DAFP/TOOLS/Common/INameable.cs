namespace DAFP.TOOLS.Common
{
    public interface INameable
    {
        string Name { get; set; }

        public static INameable Literal(string name)
        {
            return new LiteralName(name);
        }

        private class LiteralName : INameable

        {
            public LiteralName(string name)
            {
                Name = name;
            }

            public string Name { get; set; }
        }
    }
    
}