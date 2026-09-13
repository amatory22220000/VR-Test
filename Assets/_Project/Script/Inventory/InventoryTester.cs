using System.Collections.Generic;
using MeowStudio.Inventory;
using Sirenix.OdinInspector;
using UnityEngine;

namespace MeowStudio
{
    public class InventoryTester : MonoBehaviour
    {
        [SerializeField] private string ownerId;
        [SerializeField] private string itemId;
        [SerializeField] private int amount;
        [Space]
        [SerializeField]private InventoryGridView _view;

        private InventoryService _inventoryService;

        private void Start()
        {
            _inventoryService = new InventoryService();
            var inventoryData = CreateTestInventory(ownerId);
            var inventory = _inventoryService.RegisterInventory(inventoryData);
            _view.Setup(inventory);
        }

        [Button]
        private void AddItem()
        {
            TestAdd(ownerId, itemId, amount);
            _view.Print();
        }
        [Button]
        private void RemoveItem()
        {
            TestRemove(ownerId, itemId, amount);
            _view.Print();
        }
        [Button]
        private void PrintInventory()
        {
            _view.Print();
        }


        private void TestAdd(string ownerId, string itemId, int amount = 1)
        {
            var addedResult = _inventoryService.AddItemsToInventory(
                ownerId, itemId, amount);
            Debug.Log($"Items added. ItemId {itemId}, amount to add {amount}, amount added {addedResult.ItemsAddedAmount}");
        }
        private void TestRemove(string ownerId, string itemId, int amount = 1)
        {
            var removedResult = _inventoryService.RemoveItems(
                ownerId, itemId, amount);
            Debug.Log($"Items removed. ItemId {itemId}, amount to remove {amount}, success: {removedResult.Success}");
        }

        private InventoryGridData CreateTestInventory(string ownerId)
        {
            var size = new Vector2Int(3, 4);
            var createdInventorySlots = new List<InventorySlotData>();
            var lenght = size.x * size.y;
            for (int i = 0; i < lenght; i++)
                createdInventorySlots.Add(new InventorySlotData());
            var createdInventoryData = new InventoryGridData
            {
                OwnerId = ownerId,
                Size = size,
                Slots = createdInventorySlots
            };
            return createdInventoryData;
        }
    }
}
