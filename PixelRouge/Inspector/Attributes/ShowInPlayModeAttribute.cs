using System;

namespace PixelRouge.Inspector
{
    /// <summary>
    /// Show the field only if it is in Play Mode.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public sealed class ShowInPlayModeAttribute : BaseAttribute
    {
    }
}