using System;
using UnityEngine;

namespace MeowStudio.Inventory
{
    public interface IReadonlyInventoryGrid : IreadOnlyInventory
    {
        event Action<Vector2Int> SizeChanged;

        Vector2Int Size { get; }

        IReadonlyInventorySlot[,] GetSlots();
    }
}
