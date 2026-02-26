using UnityEditor.ShortcutManagement;

namespace RapidLib.DAFP.TOOLS.Editor.Shortcuts
{
    using UnityEditor;
    using UnityEngine;

// Place this file in any folder named "Editor" within your Assets directory
// e.g. Assets/Editor/PlayModeShortcut.cs

    public static class PlayModeShortcut
    {
        [Shortcut("Custom/Toggle Play Mode", KeyCode.P, ShortcutModifiers.Alt)]
        private static void TogglePlayMode()
        {
            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }
    }
}