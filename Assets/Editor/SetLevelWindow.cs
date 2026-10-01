using DreamCase.Meta;
using UnityEditor;
using UnityEngine;

namespace DreamCase.Editor
{
    /// <summary>Dream Games/Set Level: a small window to jump to any level or to "Finished".</summary>
    public sealed class SetLevelWindow : EditorWindow
    {
        [MenuItem("Dream Games/Set Level")]
        private static void Open() => GetWindow<SetLevelWindow>(true, "Set Level");

        private void OnGUI()
        {
            var progress = new ProgressService();
            string current = progress.IsFinished ? "Finished" : $"Level {progress.CurrentLevel}";
            EditorGUILayout.LabelField("Current", current, EditorStyles.boldLabel);
            EditorGUILayout.Space();

            for (int level = 1; level <= ProgressService.LevelCount; level++)
            {
                if (GUILayout.Button($"Level {level}"))
                    progress.SetCurrentLevel(level);
            }

            if (GUILayout.Button("Finished"))
                progress.SetFinished();
        }
    }
}
