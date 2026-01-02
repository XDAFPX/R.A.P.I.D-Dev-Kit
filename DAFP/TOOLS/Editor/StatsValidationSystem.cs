using DAFP.TOOLS.ECS;
using DAFP.TOOLS.ECS.BigData;
using Derkii.FindByInterfaces;
using UnityEditor.SceneManagement;

namespace RapidLib.DAFP.TOOLS.Editor
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

#if UNITY_EDITOR
    /// <summary>
    /// Validates all IEntity stats before entering play mode and on domain reload
    /// </summary>
    [InitializeOnLoad]
    public static class StatValidationSystem
    {

        static StatValidationSystem()
        {
            // Subscribe to domain reload and scene changes
            // EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
             EditorSceneManager.sceneOpened += (Scene,OpenSceneMode)=> validate_all_entities_in_scene();
            // EditorSceneManager.sceneSaved += OnSceneSaved;
            //
            // // Initial validation when scripts compile
        }


        private static void validate_all_entities_in_scene()
        {

            // Find all entities in the scene
            var _entities = InterfaceFinder.FindByInterface<IEntity>().Build().ComponentsAsInterface<IEntity>()
                .ToArray();

            if (_entities.Length == 0)
            {
                return; // No entities to validate
            }

            foreach (var _entity in _entities)
            {
                // Check if the entity has been validated

                // Perform validation
                bool _isValid = StatInjector.InjectAndValidateStats(_entity);

                if (!_isValid)
                {
                    string _error = $"IEntity '{_entity.Name}' ({_entity.GetType().Name}) has invalid or missing stats.";
                    Debug.LogError(_error, _entity.GetWorldRepresentation());
                }
            }

        }
    }

#endif
}