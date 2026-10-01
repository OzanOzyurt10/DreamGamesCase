using System;
using DreamCase.Core;
using UnityEngine;

namespace DreamCase.Meta
{
    /// <summary>Reads level JSON files from Resources/Levels.</summary>
    public static class LevelLoader
    {
        public static LevelData Load(int levelNumber)
        {
            var asset = Resources.Load<TextAsset>($"Levels/level_{levelNumber:00}");
            if (asset == null)
                throw new ArgumentException($"No level file for level {levelNumber}.", nameof(levelNumber));
            return JsonUtility.FromJson<LevelData>(asset.text);
        }
    }
}
