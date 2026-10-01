using System;
using DreamCase.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DreamCase.View
{
    /// <summary>Turns a mouse click or a touch into the board cell under it.</summary>
    public sealed class BoardInput : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private BoardView boardView;

        /// <summary>Raised on every press; the cell may be outside the board.</summary>
        public event Action<GridPosition> CellTapped;

        private void Update()
        {
            // Pointer covers both mouse and touchscreen.
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame)
                return;

            Vector2 worldPosition = worldCamera.ScreenToWorldPoint(pointer.position.ReadValue());
            CellTapped?.Invoke(boardView.WorldToCell(worldPosition));
        }
    }
}
