using UnityEngine;
using System;
using PixelRouge.Inspector.Utilities;

namespace PixelRouge.Inspector
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    public sealed class SeparatorAttribute : BaseAttribute
    {
        public Color Color = Color.gray;
        public int Thickness = 1;

        public SeparatorAttribute(int thickness = 1)
        {
            Thickness = thickness;
        }

        public SeparatorAttribute(Color color, int thickness = 1)
        {
            Color = color; 
            Thickness = thickness;
        }
    }
}