using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BigData;
using PixelRouge.Inspector;
using UnityEditor;
using UnityEngine;

namespace DAFP.TOOLS.Editor
{
    [CustomEditor(typeof(Entity), true)]
    public class EntityEditor : ImprovedEditor
    {
        private Entity _entity;
        private StatContainer _previousStats;
        private bool _statsChangedThisFrame;

        private new void OnEnable()
        {
            base.OnEnable();
            _entity = (Entity)target;
            if (_entity != null)
            {
                _previousStats = _entity.Stats;
            }
            
            // Subscribe to undo/redo events
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            if (_entity != null && _entity.Stats != _previousStats)
            {
                StatInjector.InjectAndValidateStats(_entity);
                _previousStats = _entity.Stats;
            }
        }

        public override void OnInspectorGUI()
        {
            if (_entity == null)
            {
                base.OnInspectorGUI();
                return;
            }

            serializedObject.Update();

            // Store the current Stats value before drawing
            var statsBeforeGUI = _entity.Stats;

            EditorGUI.BeginChangeCheck();
            
            // Draw the default inspector using ImprovedEditor's implementation
            base.OnInspectorGUI();

            // Check if anything changed AND specifically if Stats changed
            if (EditorGUI.EndChangeCheck())
            {
                if (_entity.Stats != statsBeforeGUI)
                {
                    StatInjector.InjectAndValidateStats(_entity);
                    _previousStats = _entity.Stats;
                    Debug.Log($"Stats reference changed for {_entity.name} - InjectAndValidateStats called");
                }
            }
        }
    }
}
