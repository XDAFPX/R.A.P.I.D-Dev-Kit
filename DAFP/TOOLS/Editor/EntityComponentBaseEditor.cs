using Archon.SwissArmyLib.ResourceSystem;
using DAFP.TOOLS.ECS;
using UnityEditor;
using UnityEngine;

namespace DAFP.TOOLS.Editor
{
    [CustomEditor(typeof(EntityComponent),true)]
    public class EntityComponentBaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var _entityComponent = (EntityComponent)target;
            if (_entityComponent is IPercentageProvider provider)
            {
                DrawProgressBar(provider);
            }

            DrawDefaultInspector();
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