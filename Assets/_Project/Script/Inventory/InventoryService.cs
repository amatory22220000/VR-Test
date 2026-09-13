using System.Collections.Generic;
using UnityEngine;

namespace MeowStudio.Inventory
{
    public class InventoryService
    {
        private readonly Dictionary<string, InventoryGrid> _inventoriesMap = new();

        public InventoryGrid RegisterInventory(InventoryGridData inventoryData)
        {
            var inventory = new InventoryGrid(inventoryData);
            _inventoriesMap[inventory.OwnerId] = inventory;

            return inventory;
        }

        public AddItemsToInventoryGridResult AddItemsToInventory(
            string ownerId,
            string itemId,
            int amount = 1)
        {
            var inventory = _inventoriesMap[ownerId];
            return inventory.AddItems(itemId, amount);
        }
        public AddItemsToInventoryGridResult AddItemsToInventorySlot(
            string ownerId,
            Vector2Int slotCoords,
            string itemId,
            int amount = 1)
        {
            var inventory = _inventoriesMap[ownerId];
            return inventory.AddItems(slotCoords, itemId, amount);
        }

        public RemoveItemsFromInventoryGridResult RemoveItems(
            string ownerId,
            string itemId,
            int amount = 1)
        {
            var inventory = _inventoriesMap[ownerId];
            return inventory.RemoveItems(itemId, amount);
        }
        public RemoveItemsFromInventoryGridResult RemoveItemsFromSlot(
            string ownerId,
            Vector2Int slotCoords,
            string itemId,
            int amount = 1)
        {
            var inventory = _inventoriesMap[ownerId];
            return inventory.RemoveItems(slotCoords, itemId, amount);
        }
        public bool Has(string ownerId, string itemId, int amount = 1)
        {
            var inventory = _inventoriesMap[ownerId];
            return inventory.Has(itemId, amount);
        }

        public IReadonlyInventoryGrid GetInventory(string ownerId)
        {
            return _inventoriesMap[ownerId];
        }
    }
}
