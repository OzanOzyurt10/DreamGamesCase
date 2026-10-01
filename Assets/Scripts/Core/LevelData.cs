using System;

namespace DreamCase.Core
{
    /// <summary>
    /// Raw contents of a level JSON file. Field names are snake_case on purpose:
    /// JsonUtility maps JSON keys to fields by exact name.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public int level_number;
        public int grid_width;
        public int grid_height;
        public int move_count;

        /// <summary>Item codes, row by row, starting from the bottom-left cell.</summary>
        public string[] grid;
    }
}
