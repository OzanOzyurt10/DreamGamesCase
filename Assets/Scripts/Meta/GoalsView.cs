using System;
using System.Collections.Generic;
using DreamCase.Core.Items;
using UnityEngine;
using UnityEngine.UI;

namespace DreamCase.Meta
{
    /// <summary>
    /// The goal box: one entry per obstacle type the level starts with. Maps obstacle types to icons through a
    /// registry, like ItemViewFactory does for item views.
    /// </summary>
    public sealed class GoalsView : MonoBehaviour
    {
        [SerializeField] private GoalEntryView entryTemplate;
        [SerializeField] private GridLayoutGroup grid;

        [SerializeField] private Sprite vaseIcon;
        [SerializeField] private Sprite stoneIcon;
        [SerializeField] private Sprite chaliceBoxIcon;

        [Tooltip("Cell size when the level has a single goal, and when it has several.")]
        [SerializeField] private float singleGoalCellSize = 190f;
        [SerializeField] private float multiGoalCellSize = 110f;

        private readonly Dictionary<Type, GoalEntryView> entries = new Dictionary<Type, GoalEntryView>();

        /// <summary>Creates an entry for every obstacle type on the starting board.</summary>
        public void Initialize(IReadOnlyDictionary<Type, int> remaining)
        {
            var icons = new Dictionary<Type, Sprite>
            {
                [typeof(Vase)] = vaseIcon,
                [typeof(Stone)] = stoneIcon,
                [typeof(ChaliceBox)] = chaliceBoxIcon,
            };

            grid.cellSize = Vector2.one * (remaining.Count == 1 ? singleGoalCellSize : multiGoalCellSize);

            foreach (var type in remaining.Keys)
            {
                if (!icons.TryGetValue(type, out var icon))
                    throw new InvalidOperationException($"No goal icon registered for {type.Name}.");

                var entry = Instantiate(entryTemplate, entryTemplate.transform.parent);
                entry.gameObject.SetActive(true);
                entry.SetIcon(icon);
                entries.Add(type, entry);
            }
            Refresh(remaining);
        }

        /// <summary>Types missing from <paramref name="remaining"/> are fully cleared and show a check mark.</summary>
        public void Refresh(IReadOnlyDictionary<Type, int> remaining)
        {
            foreach (var pair in entries)
                pair.Value.Show(remaining.TryGetValue(pair.Key, out int count) ? count : 0);
        }
    }
}
