using Archon.SwissArmyLib.ResourceSystem;
using DAFP.TOOLS.ECS;
using PixelRouge.Inspector;
using UnityEditor;
using UnityEngine;

namespace DAFP.TOOLS.Editor
{
    [CustomEditor(typeof(Entity), true)]
    public class EntityBaseEditor : ImprovedEditor
    {
        public override void OnInspectorGUI()
        {
            var _entityComponent = (Entity)target;
            DisableEverything= _entityComponent.IsReadOnly;
            base.OnInspectorGUI();

            if (_entityComponent is IPercentageProvider provider)
            {
                DrawProgressBar(provider);
            }

            // EditorGUILayout.Separator();
            // EditorGUILayout.BeginHorizontal();
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