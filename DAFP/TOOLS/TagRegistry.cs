
public static class TagRegistry
{
    public static readonly string[] AllTags = new string[] { "Untagged", "Respawn", "Finish", "EditorOnly", "MainCamera", "Player", "GameController", "ASCIICamera" };

    public enum Tag
    {
        Untagged,
        Respawn,
        Finish,
        EditorOnly,
        MainCamera,
        Player,
        GameController,
        ASCIICamera
    }

    public static string GetTagString(Tag t) => AllTags[(int)t];
}
