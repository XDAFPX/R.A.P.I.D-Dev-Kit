﻿using Archon.SwissArmyLib.ResourceSystem;
using DAFP.TOOLS.ECS;
using PixelRouge.Inspector;
using UnityEditor;
using UnityEngine;

namespace DAFP.TOOLS.Editor
{
    [CustomEditor(typeof(EntityComponent), true)]
    public class EntityComponentBaseEditor : ImprovedEditor
    {
        public override void OnInspectorGUI()
        {
            var _entityComponent = (EntityComponent)target;
            if (_entityComponent is IPercentageProvider provider)
            {
                DrawProgressBar(provider);
            }

            DisableEverything = false;
            if (_entityComponent.Host != null)
                if (_entityComponent.Host is Entity ent)
                    DisableEverything = ent.IsReadOnly;

            base.OnInspectorGUI();
        }

        private static void DrawProgressBar(IPercentageProvider provider)
        {
            EditorGUILayout.Separator();
            EditorGUILayout.BeginHorizontal();
            // Use EditorGUIUtility.currentViewWidth for a reasonable width
            var barRect = GUILayoutUtility.GetRect(EditorGUIUtility.currentViewWidth - 40, 20);
            EditorGUI.ProgressBar(barRect, provider.Percentage,
                $"-- {provider.GetFormatedCurValue()} / {provider.GetFormatedMaxValue()} --");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }
    }
}