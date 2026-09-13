using System;

namespace MeowStudio.Inventory
{
    public interface IreadOnlyInventory
    {
        event Action<string, int> ItemsAdded;
        event Action<string, int> ItemsRemoved;

        string OwnerId { get; }

        int GetAmount(string itemID);
        bool Has(string itemID, int amount);
    }
}
