﻿using UnityEngine;
using UnityEditor;

namespace PixelRouge.Inspector
{
    [CustomPropertyDrawer(typeof(LeftToggleAttribute))]
    public class LeftToggleAttributeDrawer : BasePropertyDrawer 
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) 
        {
            DrawFieldWithToggleOnTheLeft(position, property, label);
        }
    }

}