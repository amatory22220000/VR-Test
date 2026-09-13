using UnityEngine;

namespace MeowStudio.Inventory
{
    public class InventoryGridView: MonoBehaviour
    {
        private IReadonlyInventoryGrid _inventory;

        public void Setup(IReadonlyInventoryGrid inventory)
        {
            _inventory = inventory;
            Print();

        }

        public void Print()
        {
            var slots = _inventory.GetSlots();
            var size = _inventory.Size;
            var result = "";


            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    var slot = slots[i, j];
                    result += $"Slot ({i}:{j}). Item: {slot.ItemId}. Amount: {slot.Amount}\n";
                }
            }

            Debug.Log(result);
        }
    }
}
