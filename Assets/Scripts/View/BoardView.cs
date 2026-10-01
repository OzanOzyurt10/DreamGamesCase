using System.Collections.Generic;
using DG.Tweening;
using DreamCase.Core;
using DreamCase.Core.Items;
using UnityEngine;

namespace DreamCase.View
{
    /// <summary>
    /// Draws a board and animates the changes the model reports. Keeps the link from each model item
    /// to its view; contains no game rules.
    /// </summary>
    public sealed class BoardView : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private ItemViewFactory itemViewFactory;
        [SerializeField] private SpriteRenderer frame;
        [SerializeField] private SpriteMask itemMask;
        [SerializeField] private Transform itemsRoot;

        [Header("Layout")]
        [Tooltip("Part of the screen height kept free at the top for the goal / move panel.")]
        [SerializeField, Range(0f, 0.5f)] private float topReservedFraction = 0.2f;

        [Tooltip("Empty space around the board, as a fraction of the screen width.")]
        [SerializeField, Range(0f, 0.2f)] private float screenMarginFraction = 0.04f;

        [Tooltip("Thickness of the frame around the cells, in cells.")]
        [SerializeField, Range(0f, 1f)] private float framePaddingInCells = 0.15f;

        [Header("Animation")]
        [SerializeField] private float blastDuration = 0.2f;

        [Tooltip("Time for a group's cubes to slide into the tapped cell before a special item appears.")]
        [SerializeField] private float mergeDuration = 0.25f;

        [SerializeField] private float specialAppearDuration = 0.2f;

        [Tooltip("Time to fall one cell. Longer falls take longer, growing with the square root of the distance like real gravity.")]
        [SerializeField] private float fallDurationPerCell = 0.12f;

        [Tooltip("How high an item bounces after landing, in cells.")]
        [SerializeField] private float landingBounceInCells = 0.08f;

        [SerializeField] private float landingBounceDuration = 0.12f;

        private readonly Dictionary<Item, ItemView> views = new Dictionary<Item, ItemView>();
        private BoardLayout layout;

        public void Build(Board board)
        {
            layout = new BoardLayout(board.Width, board.Height, AvailableArea(), framePaddingInCells);

            frame.transform.position = layout.Center;
            frame.size = layout.FrameSize;

            itemMask.transform.position = layout.Center;
            Vector2 maskSpriteSize = itemMask.sprite.bounds.size;
            itemMask.transform.localScale = new Vector3(
                layout.FrameSize.x / maskSpriteSize.x,
                layout.FrameSize.y / maskSpriteSize.y,
                1f);

            foreach (var item in board.Items)
                CreateView(item, layout.ItemCenter(item));
        }

        public GridPosition WorldToCell(Vector2 worldPosition) => layout.WorldToCell(worldPosition);

        /// <summary>
        /// Small groups shrink away. Groups that earned a special item slide into the tapped cell first,
        /// then the special item pops up there. Blasted views go back to the pool.
        /// </summary>
        public Tween AnimateBlast(BlastResult result)
        {
            var sequence = DOTween.Sequence();
            var removed = new List<(Item item, ItemView view)>();
            Vector2 tappedCellCenter = layout.CellCenter(result.TappedCell);
            bool merging = result.CreatedItem != null;

            foreach (var cube in result.BlastedCubes)
            {
                var view = views[cube];
                views.Remove(cube);
                removed.Add((cube, view));
                sequence.Join(merging ? view.PlayMerge(tappedCellCenter, mergeDuration) : view.PlayBlast(blastDuration));
            }

            sequence.AppendCallback(() =>
            {
                foreach (var (item, view) in removed)
                    itemViewFactory.Release(item, view);
            });

            if (merging)
                sequence.Append(CreateView(result.CreatedItem, tappedCellCenter).PlayAppear(specialAppearDuration));

            return sequence;
        }

        /// <summary>Shows on every cube which special item its group would create.</summary>
        public void ShowHints(IReadOnlyDictionary<Cube, BlastReward> hints)
        {
            foreach (var pair in views)
            {
                var reward = pair.Key is Cube cube && hints.TryGetValue(cube, out var hint) ? hint : BlastReward.None;
                pair.Value.ShowHint(reward);
            }
        }

        /// <summary>Drops fallen items to their new cells and new cubes in from above the board, all at once.</summary>
        public Tween AnimateFalls(FallResult result)
        {
            var sequence = DOTween.Sequence();

            foreach (var fall in result.Falls)
                sequence.Join(PlayFall(views[fall.Item], fall));

            foreach (var spawn in result.Spawns)
                sequence.Join(PlayFall(CreateView(spawn.Item, layout.CellCenter(spawn.From)), spawn));

            return sequence;
        }

        private ItemView CreateView(Item item, Vector2 worldPosition)
        {
            var view = itemViewFactory.Create(item, itemsRoot);
            view.transform.position = worldPosition;
            view.FitWidth(item.Width * layout.CellSize);
            // Higher rows draw on top, so a cube's bottom edge overlaps the cube below it.
            view.SetSortingOrder(item.Position.Y);
            views.Add(item, view);
            return view;
        }

        private Tween PlayFall(ItemView view, ItemFall fall)
        {
            view.SetSortingOrder(fall.To.Y);
            int distance = fall.From.Y - fall.To.Y;
            return view.PlayFall(
                layout.CellCenter(fall.To),
                fallDurationPerCell * Mathf.Sqrt(distance),
                landingBounceInCells * layout.CellSize,
                landingBounceDuration);
        }

        /// <summary>The world-space rectangle the board may use: the screen minus margins and the top panel.</summary>
        private Rect AvailableArea()
        {
            float screenHeight = worldCamera.orthographicSize * 2f;
            float screenWidth = screenHeight * worldCamera.aspect;
            float margin = screenWidth * screenMarginFraction;

            Vector2 cameraCenter = worldCamera.transform.position;
            float left = cameraCenter.x - screenWidth / 2f + margin;
            float bottom = cameraCenter.y - screenHeight / 2f + margin;
            float width = screenWidth - 2f * margin;
            float height = screenHeight * (1f - topReservedFraction) - 2f * margin;

            return new Rect(left, bottom, width, height);
        }
    }
}
