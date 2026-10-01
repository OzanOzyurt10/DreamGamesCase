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
        [SerializeField] private Transform itemsRoot;

        [Tooltip("Part of the screen height kept free at the top for the goal / move panel.")]
        [SerializeField, Range(0f, 0.5f)] private float topReservedFraction = 0.2f;

        [Tooltip("Empty space around the board, as a fraction of the screen width.")]
        [SerializeField, Range(0f, 0.2f)] private float screenMarginFraction = 0.04f;

        [Tooltip("Thickness of the frame around the cells, in cells.")]
        [SerializeField, Range(0f, 1f)] private float framePaddingInCells = 0.15f;

        [SerializeField] private float blastDuration = 0.2f;

        private readonly Dictionary<Item, ItemView> views = new Dictionary<Item, ItemView>();
        private BoardLayout layout;

        public void Build(Board board)
        {
            layout = new BoardLayout(board.Width, board.Height, AvailableArea(), framePaddingInCells);

            frame.transform.position = layout.Center;
            frame.size = layout.FrameSize;

            foreach (var item in board.Items)
            {
                var view = itemViewFactory.Create(item, itemsRoot);
                view.transform.position = layout.ItemCenter(item);
                view.FitWidth(item.Width * layout.CellSize);
                // Higher rows draw on top, so a cube's bottom edge overlaps the cube below it.
                view.SetSortingOrder(item.Position.Y);
                views.Add(item, view);
            }
        }

        public GridPosition WorldToCell(Vector2 worldPosition) => layout.WorldToCell(worldPosition);

        /// <summary>Shrinks the blasted cubes away, then returns their views to the pool.</summary>
        public Tween AnimateBlast(BlastResult result)
        {
            var sequence = DOTween.Sequence();
            var removed = new List<(Item item, ItemView view)>();

            foreach (var cube in result.BlastedCubes)
            {
                var view = views[cube];
                views.Remove(cube);
                removed.Add((cube, view));
                sequence.Join(view.PlayBlast(blastDuration));
            }

            sequence.OnComplete(() =>
            {
                foreach (var (item, view) in removed)
                    itemViewFactory.Release(item, view);
            });
            return sequence;
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
