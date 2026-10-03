using DreamCase.Core.Items;
using UnityEngine;

namespace DreamCase.View
{
    /// <summary>
    /// Shows the closed door, or once it is open, the shelves with the chalices still left in the box.
    /// The chalice icons are child sprites laid out on two shelves, bottom shelf first.
    /// </summary>
    public sealed class ChaliceBoxView : ItemView
    {
        private const int ShelfCount = 2;
        private const int ChalicesPerShelf = ChaliceBox.ChaliceCount / ShelfCount;

        [SerializeField] private Sprite door;
        [SerializeField] private Sprite shelves;
        [SerializeField] private Sprite chalice;

        [Header("Chalice layout, in the box sprite's local units")]
        [Tooltip("Height of a chalice's center on the bottom and top shelf.")]
        [SerializeField] private float bottomShelfY = -0.72f;
        [SerializeField] private float topShelfY = 0.45f;
        [Tooltip("Distance between the first and last chalice on a shelf.")]
        [SerializeField] private float shelfWidth = 1.9f;
        [SerializeField] private float chaliceScale = 0.55f;

        private SpriteRenderer[] chalices;

        protected override void Awake()
        {
            base.Awake();
            chalices = new SpriteRenderer[ChaliceBox.ChaliceCount];
            for (int i = 0; i < chalices.Length; i++)
                chalices[i] = CreateChalice(i);
        }

        public override void Bind(Item item)
        {
            var box = (ChaliceBox)item;
            SpriteRenderer.sprite = box.IsDoorOpen ? shelves : door;

            // The top shelf empties first.
            for (int i = 0; i < chalices.Length; i++)
                chalices[i].enabled = box.IsDoorOpen && i < box.ChalicesLeft;
        }

        public override void SetSortingOrder(int order)
        {
            base.SetSortingOrder(order);
            foreach (var icon in chalices)
                icon.sortingOrder = order + 1;
        }

        private SpriteRenderer CreateChalice(int index)
        {
            int shelf = index / ChalicesPerShelf;
            int slot = index % ChalicesPerShelf;
            float x = -shelfWidth / 2f + slot * shelfWidth / (ChalicesPerShelf - 1);
            float y = shelf == 0 ? bottomShelfY : topShelfY;

            var icon = new GameObject("Chalice").AddComponent<SpriteRenderer>();
            icon.transform.SetParent(transform, false);
            icon.transform.localPosition = new Vector3(x, y, 0f);
            icon.transform.localScale = Vector3.one * chaliceScale;
            icon.sprite = chalice;
            icon.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
            return icon;
        }
    }
}
