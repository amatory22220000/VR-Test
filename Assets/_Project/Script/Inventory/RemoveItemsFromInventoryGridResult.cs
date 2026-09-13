namespace MeowStudio.Inventory
{
    public readonly struct RemoveItemsFromInventoryGridResult
    {
        public readonly string InventoryOwnerId;
        public readonly int ItemsToAddAmount;
        public readonly bool Success;

        public RemoveItemsFromInventoryGridResult(
            string inventoryOwnerId,
            int itemsToAddAmount,
            bool success)
        {
            InventoryOwnerId = inventoryOwnerId;
            ItemsToAddAmount = itemsToAddAmount;
            Success = success;
        }
    }
}
