using System;
using System.Collections.Generic;
using UnityEngine;

namespace MeowStudio.Inventory
{
    public class InventoryGrid : IReadonlyInventoryGrid
    {
        public event Action<Vector2Int> SizeChanged;
        public event Action<string, int> ItemsAdded;
        public event Action<string, int> ItemsRemoved;

        public Vector2Int Size
        {
            get => _data.Size;
            set
            {
                if (_data.Size != value)
                {
                    _data.Size = value;
                    SizeChanged?.Invoke(value);
                }
            }
        }

        public string OwnerId => _data.OwnerId;

        private readonly InventoryGridData _data;
        private readonly Dictionary<Vector2Int, InventorySlot> _slotsMap;

        public InventoryGrid(InventoryGridData data)
        {
            _data = data;
            var size = data.Size;
            _slotsMap = new Dictionary<Vector2Int, InventorySlot>();

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    var index = i * size.y + j;
                    var slotData = data.Slots[index];
                    var slot = new InventorySlot(slotData);
                    var position = new Vector2Int(i, j);

                    _slotsMap.Add(position, slot);
                }
            }
        }

        public AddItemsToInventoryGridResult AddItems(
            string itemId,
            int amount = 1)
        {
            var remainingAmount = amount;
            var addedItemsAmount = 0;

            //Add to same items not full filled slots
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    var coords = new Vector2Int(i, j);
                    var slot = _slotsMap[coords];
                    var slotCapacity = GetItemSlotCapacity(slot.ItemId);

                    if (slot.IsEmpty)
                        continue;
                    if (slot.Amount == slotCapacity)
                        continue;
                    if (slot.ItemId != itemId)
                        continue;

                    var canAddAmount = slotCapacity - slot.Amount;

                    //can add all amount to this slot
                    if (remainingAmount <= canAddAmount)
                    {
                        addedItemsAmount += remainingAmount;
                        slot.Amount += remainingAmount;
                        remainingAmount = 0;

                        return new AddItemsToInventoryGridResult(
                            itemId,
                            remainingAmount,
                            addedItemsAmount);
                    }
                    //add not all amount to this slot
                    else
                    {
                        remainingAmount -= canAddAmount;
                        addedItemsAmount += canAddAmount;
                        slot.Amount = slotCapacity;
                    }
                }
            }
            //Add to empty slots
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    var coords = new Vector2Int(i, j);
                    var slot = _slotsMap[coords];
                    var slotCapacity = GetItemSlotCapacity(slot.ItemId);

                    if (!slot.IsEmpty)
                        continue;

                    //can add all amount to this slot
                    if (remainingAmount <= slotCapacity)
                    {
                        addedItemsAmount += remainingAmount;
                        slot.ItemId = itemId;
                        slot.Amount += remainingAmount;
                        remainingAmount = 0;

                        return new AddItemsToInventoryGridResult(
                            itemId,
                            remainingAmount,
                            addedItemsAmount);
                    }
                    //add not all amount to this slot
                    else
                    {
                        remainingAmount += slotCapacity;
                        slot.ItemId = itemId;
                        slot.Amount = slotCapacity;
                        remainingAmount -= slotCapacity;
                    }
                }
            }
            return new AddItemsToInventoryGridResult(
                itemId,
                amount,
                addedItemsAmount);
        }
        public AddItemsToInventoryGridResult AddItems(
            Vector2Int slotCoord,
            string itemId,
            int amount = 1)
        {
            var slot = _slotsMap[slotCoord];
            var newValue = slot.Amount + amount;
            var itemsAddedAmount = 0;
            if(slot.IsEmpty)
                slot.ItemId = itemId;
            var itemSlotCapacity = GetItemSlotCapacity(slot.ItemId);
            if(newValue > itemSlotCapacity)
            {
                var remainingItems = newValue - itemSlotCapacity;
                var itemsToAddAmount = itemsAddedAmount - slot.Amount;
                itemsAddedAmount += itemsToAddAmount;
                slot.Amount = itemSlotCapacity;

                var result = AddItems(itemId, remainingItems);
                itemsAddedAmount += result.ItemsAddedAmount;
            }
            else
            {
                itemsAddedAmount = amount;
                slot.Amount = newValue;
            }

            return new AddItemsToInventoryGridResult(OwnerId, amount, itemsAddedAmount);
        }


        public RemoveItemsFromInventoryGridResult RemoveItems(
            string itemId,
            int amount = 1)
        {
            if (!Has(itemId, amount))
                return new RemoveItemsFromInventoryGridResult(
                    OwnerId,
                    amount,
                    false);
            var amountToRemove = amount;

            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    var slotCoords = new Vector2Int(i, j);
                    var slot = _slotsMap[slotCoords];

                    if (slot.ItemId != itemId)
                        continue;

                    if(amountToRemove > slot.Amount)
                    {
                        amountToRemove -= slot.Amount;
                        RemoveItems(slotCoords, itemId, slot.Amount);
                        if (amountToRemove == 0)
                            return new RemoveItemsFromInventoryGridResult(
                                OwnerId,
                                amount,
                                true);
                    }
                    else
                    {
                        RemoveItems(slotCoords, itemId, amountToRemove);
                        return new RemoveItemsFromInventoryGridResult(
                            OwnerId,
                            amount,
                            true);
                    }
                }
            }


            throw new Exception("Something went wrong, couldn't remove some items");
        }
        public RemoveItemsFromInventoryGridResult RemoveItems(
            Vector2Int slotCoord,
            string itemId,
            int amount = 1)
        {
            var slot = _slotsMap[slotCoord];
            if (slot.IsEmpty || slot.ItemId != itemId || slot.Amount < amount)
                return new RemoveItemsFromInventoryGridResult(OwnerId, amount, false);

            slot.Amount -= amount;
            if(slot.Amount == 0)
                slot.ItemId = null;
            return new RemoveItemsFromInventoryGridResult(OwnerId, amount, true);
        }

        private int GetItemSlotCapacity(string itemId)
        {
            return 10;
        }
        public int GetAmount(string itemID)
        {
            var amount = 0;
            var slots = _data.Slots;
            foreach (var slot in slots)
            {
                if (slot.ItemId == itemID)
                    amount += slot.Amount;
            }
            return amount;
        }
        public bool Has(string itemID, int amount)
        {
            var amountExists = GetAmount(itemID);
            return amountExists >= amount;
        }
        public bool HasFreeSlots()
        {
            var slots = _data.Slots;
            foreach (var slot in slots)
            {
                if (string.IsNullOrEmpty(slot.ItemId) && slot.Amount == 0)
                    return true;
            }
            return false;
        }
        public bool HasSameItemsNotFullSlots(string itemId)
        {
            var slots = _data.Slots;
            foreach (var slot in slots)
            {
                if (slot.Amount >= GetItemSlotCapacity(itemId)) continue;
                if (slot.ItemId != itemId) continue;
                return true;
            }
            return false;
        }
        public void SwitchSlots(Vector2Int slotCoordA, Vector2Int slotCoordB)
        {
            var slotA = _slotsMap[slotCoordA];
            var slotB = _slotsMap[slotCoordB];

            var tempSlotItemId = slotA.ItemId;
            var tempSlotItemAmount= slotA.Amount;

            slotA.ItemId = slotB.ItemId;
            slotA.Amount = slotB.Amount;

            slotB.ItemId = tempSlotItemId;
            slotB.Amount = tempSlotItemAmount;
        }
        public void SetSize(Vector2Int newSize)
        {
            throw new NotImplementedException();
        }

        public IReadonlyInventorySlot[,] GetSlots()
        {
            var array = new IReadonlyInventorySlot[Size.x, Size.y];
            for (int i = 0; i < Size.x; i++)
            {
                for (int j = 0; j < Size.y; j++)
                {
                    var position = new Vector2Int(i, j);
                    array[i, j] = _slotsMap[position];
                }
            }
            return array;
        }
    }
}
