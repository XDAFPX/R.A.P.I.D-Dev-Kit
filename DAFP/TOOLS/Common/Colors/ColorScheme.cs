using System.Linq;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace DAFP.TOOLS.Common.Colors
{
    /// <summary>
    /// Abstract base class for any color scheme.
    /// Defines the structure for how color schemes should behave.
    /// </summary>
    public abstract class ColorScheme
    {
        /// <summary>
        /// List of colors that belong to this scheme.
        /// </summary>
        protected List<Color> colors = new();

        public ColorScheme(Color based)
        {
            Generate(based);
        }

        /// <summary>
        /// Number of colors the scheme contains.
        /// </summary>
        public abstract int ColorCount { get; }

        /// <summary>
        /// Returns a color by index (0-based).
        /// </summary>
        public virtual Color GetColor(int index)
        {
            if (index < 0 || index >= colors.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Color index out of range.");
            return colors[index];
        }

        /// <summary>
        /// Returns all colors in the scheme.
        /// </summary>
        public virtual IReadOnlyList<Color> GetAllColors()
        {
            return colors.AsReadOnly();
        }

        /// <summary>
        /// Regenerates the colors (for example, if the base color changes).
        /// </summary>
        public abstract void Generate(Color baseColor);

        /// <summary>
        /// A monochrome scheme — variations of a single hue.
        /// </summary>
        public class MonochromeScheme : ColorScheme
        {
            public override int ColorCount => 3;

            public override void Generate(Color baseColor)
            {
                colors.Clear();
                colors.Add(baseColor);
                colors.Add(baseColor * 0.5f); // darker variant
                colors.Add(Color.Lerp(baseColor, Color.white, 0.5f)); // lighter variant
            }

            public MonochromeScheme(Color based) : base(based)
            {
            }
        }
    }


    /// <summary>
    /// A complementary scheme — base color and its opposite.
    /// </summary>
    public class ComplementaryScheme : ColorScheme
    {
        public override int ColorCount => 2;

        public override void Generate(Color baseColor)
        {
            colors.Clear();
            colors.Add(baseColor);
            Color.RGBToHSV(baseColor, out var h, out var s, out var v);
            var oppositeHue = (h + 0.5f) % 1f;
            colors.Add(Color.HSVToRGB(oppositeHue, s, v));
        }

        public ComplementaryScheme(Color based) : base(based)
        {
        }
    }


    /// <summary>
    /// Analogous colors — neighboring hues on the color wheel.
    /// </summary>
    public class AnalogousScheme : ColorScheme
    {
        public override int ColorCount => 3;

        public override void Generate(Color baseColor)
        {
            colors.Clear();
            Color.RGBToHSV(baseColor, out var h, out var s, out var v);

            var step = 1f / 12f; // 30 degrees on hue wheel
            colors.Add(Color.HSVToRGB((h - step + 1f) % 1f, s, v));
            colors.Add(baseColor);
            colors.Add(Color.HSVToRGB((h + step) % 1f, s, v));
        }

        public AnalogousScheme(Color based) : base(based)
        {
        }
    }

    public class SimpleScheme : ColorScheme
    {
        private readonly IEnumerable<Color> schemecolors;

        public SimpleScheme(IEnumerable<Color> mainColors) : base(Color.black)
        {
            schemecolors = mainColors;
            colors = schemecolors.ToList();
        }

        public override int ColorCount => schemecolors.Count();

        public override void Generate(Color baseColor)
        {
            colors = schemecolors.ToList();
        }
    }
}