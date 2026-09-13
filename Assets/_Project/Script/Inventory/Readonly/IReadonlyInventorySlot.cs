using System;

namespace MeowStudio.Inventory
{
    public interface IReadonlyInventorySlot
    {
        event Action<string> ItemIdChanged;
        event Action<int> ItemAmountChanged;

        string ItemId { get; }
        int Amount { get; }
        bool IsEmpty { get; }
    }
}
