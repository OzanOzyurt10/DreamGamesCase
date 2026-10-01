using System;
using System.Collections.Generic;
using DreamCase.Core.Items;
using UnityEngine;
using UnityEngine.Pool;

namespace DreamCase.View
{
    /// <summary>
    /// Maps each model item type to its view prefab and keeps a pool per type, so removed views are
    /// reused instead of destroyed. Mirrors ItemFactory on the model side: a new item type needs one
    /// prefab field and one registry entry.
    /// </summary>
    public sealed class ItemViewFactory : MonoBehaviour
    {
        [SerializeField] private CubeView cubePrefab;
        [SerializeField] private RocketView rocketPrefab;
        [SerializeField] private ItemView tntPrefab;
        [SerializeField] private ItemView vasePrefab;
        [SerializeField] private ItemView stonePrefab;
        [SerializeField] private ItemView chaliceBoxPrefab;

        private Dictionary<Type, ObjectPool<ItemView>> pools;

        private void Awake()
        {
            pools = new Dictionary<Type, ObjectPool<ItemView>>
            {
                [typeof(Cube)] = CreatePool(cubePrefab),
                [typeof(Rocket)] = CreatePool(rocketPrefab),
                [typeof(Tnt)] = CreatePool(tntPrefab),
                [typeof(Vase)] = CreatePool(vasePrefab),
                [typeof(Stone)] = CreatePool(stonePrefab),
                [typeof(ChaliceBox)] = CreatePool(chaliceBoxPrefab),
            };
        }

        public ItemView Create(Item item, Transform parent)
        {
            var view = PoolFor(item).Get();
            view.transform.SetParent(parent, false);
            view.Bind(item);
            return view;
        }

        public void Release(Item item, ItemView view) => PoolFor(item).Release(view);

        private ObjectPool<ItemView> PoolFor(Item item)
        {
            if (!pools.TryGetValue(item.GetType(), out var pool))
                throw new InvalidOperationException($"No view prefab registered for {item.GetType().Name}.");
            return pool;
        }

        private static ObjectPool<ItemView> CreatePool(ItemView prefab) =>
            new ObjectPool<ItemView>(
                createFunc: () => Instantiate(prefab),
                actionOnGet: view => view.gameObject.SetActive(true),
                actionOnRelease: view => view.gameObject.SetActive(false),
                actionOnDestroy: view => Destroy(view.gameObject));
    }
}
