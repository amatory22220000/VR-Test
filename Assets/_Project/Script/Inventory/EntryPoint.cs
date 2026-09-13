// using System.Collections.Generic;
//using UnityEngine;

//namespace MeowStudio.Inventory
//{
//    public class EntryPoint: MonoBehaviour
//    {
//        public InventoryGridView _view;
//        public InventoryService _inventoryService;

//        private string testOwner = "PlayerYurii";
//        private void Start()
//        {
//            _inventoryService = new InventoryService();
//            var ownerId = testOwner;
//            var inventoryData = CreateTestInventory(ownerId);
//            var inventory = _inventoryService.RegisterInventory(inventoryData);

//            _view.Setup(inventory);

//            //TestAdd(testOwner, "apple", 310);
//            //TestRemove(testOwner, "apple", 25);

//            //TestAdd(testOwner, "brick", 199);
//            //TestRemove(testOwner, "brick", 210);

//            //_view.Print();
//        }

//        private InventoryGridData CreateTestInventory(string ownerId)
//        {
//            var size = new Vector2Int(3, 4);
//            var createdInventorySlots = new List<InventorySlotData>();
//            var lenght = size.x * size.y;
//            for (int i = 0; i < lenght; i++)
//                createdInventorySlots.Add(new InventorySlotData());
//             var createdInventoryData = new InventoryGridData
//            {
//                OwnerId = ownerId,
//                Size = size,
//                Slots = createdInventorySlots
//            };
//            return createdInventoryData;
//        }

//        private void TestAdd(string ownerId, string itemId, int amount = 1)
//        {
//            var addedResult = _inventoryService.AddItemsToInventory(
//                ownerId, itemId, amount);
//            Debug.Log($"Items added. ItemId {itemId}, amount to add {amount}, amount added {addedResult.ItemsAddedAmount}");
//        }
//        private void TestRemove(string ownerId, string itemId, int amount = 1)
//        {
//            var removedResult = _inventoryService.RemoveItems(
//                ownerId, itemId, amount);
//            Debug.Log($"Items removed. ItemId {itemId}, amount to remove {amount}, success: {removedResult.Success}");
//        }
//    }
//}
