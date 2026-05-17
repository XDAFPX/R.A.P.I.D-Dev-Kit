
public static class GameAssetsRegistry
{
    public static readonly string[] AllAddresses = new string[] { "GAME.Assets.Effects.HitEffect01", "GAME.Assets.Effects.PS_Damage01", "GAME.Assets.Effects.PS_Damage02", "GAME.Assets.Effects.PS_Ripple01", "GAME.Assets.Effects.PS_Ripple02", "GAME.Assets.Effects.PS_Splash01", "GAME.Assets.Effects.PS_Water01" };

    public enum Prefix
    {
        Effects
    }
    private static readonly string[] PrefixStrings = new string[] { "Effects" };
    public static string GetPrefixString(Prefix p) => PrefixStrings[(int)p];
        public static class Effects
        {
            public const string Prefix = "Effects";
            public static readonly string[] UNames = new string[] { "HitEffect01", "PS_Damage01", "PS_Damage02", "PS_Ripple01", "PS_Ripple02", "PS_Splash01", "PS_Water01" };
            public static readonly string[] Addresses = new string[] { "GAME.Assets.Effects.HitEffect01", "GAME.Assets.Effects.PS_Damage01", "GAME.Assets.Effects.PS_Damage02", "GAME.Assets.Effects.PS_Ripple01", "GAME.Assets.Effects.PS_Ripple02", "GAME.Assets.Effects.PS_Splash01", "GAME.Assets.Effects.PS_Water01" };
            public enum N
            {
            HitEffect01,
            PS_Damage01,
            PS_Damage02,
            PS_Ripple01,
            PS_Ripple02,
            PS_Splash01,
            PS_Water01
            }
            public static string GetUNameString(N u) => UNames[(int)u];
            public static string GetAddress(N u) => Addresses[(int)u];
        }

}
