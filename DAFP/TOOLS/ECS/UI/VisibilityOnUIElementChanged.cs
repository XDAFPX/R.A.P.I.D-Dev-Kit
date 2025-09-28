namespace DAFP.TOOLS.ECS.UI
{
    public struct VisibilityOnUIElementChanged
    {
        public VisibilityOnUIElementChanged(IUIElement element, bool visibilty)
        {
            Element = element;
            Visibilty = visibilty;
        }

        public readonly IUIElement Element { get; init; }
        public readonly bool Visibilty { get; init; }
    }
}