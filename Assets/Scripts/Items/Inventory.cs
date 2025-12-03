using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class Inventory
{
    public enum ItemPlaceResponse
    { Placed, Stacked, Replaced, Blocked };

    public event Action<ItemInstance, int, int> OnItemAdded = delegate { };

    public event Action<ItemInstance> OnItemRemoved = delegate { };

    public int SizeX => sizeX;
    public int SizeY => sizeY;

    public Inventory(int sizeX, int sizeY)
    {
        items = new List<ItemInstance>();
        slots = new int[sizeX, sizeY];
        this.sizeX = sizeX;
        this.sizeY = sizeY;

        // Initialize slots to -1
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                slots[x, y] = -1;
            }
        }
    }

    public ItemPlaceResponse TryQuickStackItem(ItemInstance item)
    {
        ItemPlaceResponse response = ItemPlaceResponse.Blocked;

        // While can stack item, stack it
        while (true)
        {
            bool found = false;

            foreach (ItemInstance i in items)
            {
                if (i.Data == item.Data && StackItem(i, item))
                {
                    response = ItemPlaceResponse.Stacked;
                    found = true;
                }
            }

            if (!found) break;
        }

        // If items are still left try place
        if (item.Amount > 0)
        {
            bool found = false;
            for (int x = 0; x < sizeX && !found; x++)
            {
                for (int y = 0; y < sizeY && !found; y++)
                {
                    if (PlaceItem(item, x, y))
                    {
                        response = ItemPlaceResponse.Placed;
                        found = true;
                    }
                }
            }
        }

        return response;
    }

    public (ItemPlaceResponse, ItemInstance) TryPlaceItem(ItemInstance item, int x, int y)
    {
        // Check position is in bounds
        if (x < 0 || y < 0 || x + item.Data.SizeX > sizeX || y + item.Data.SizeY > sizeY)
        {
            return (ItemPlaceResponse.Blocked, null);
        }

        // Check if item under cursor matches and stack
        if (slots[x, y] != -1)
        {
            ItemInstance existingItem = items[slots[x, y]];
            if (existingItem.Data == item.Data)
            {
                if (StackItem(existingItem, item)) return (ItemPlaceResponse.Stacked, null);
            }
        }

        // Find number of overlapping items
        HashSet<int> overlappingItems = new();
        for (int i = 0; i < item.Data.SizeX; i++)
        {
            for (int j = 0; j < item.Data.SizeY; j++)
            {
                if (x + i >= sizeX || y + j >= sizeY || slots[x + i, y + j] == -1) continue;
                overlappingItems.Add(slots[x + i, y + j]);
            }
        }

        // Overlapping 2+ items, therefore blocked
        if (overlappingItems.Count > 1)
        {
            return (ItemPlaceResponse.Blocked, null);
        }

        // Overlapping 1 item
        else if (overlappingItems.Count == 1)
        {
            var existingItemIndex = overlappingItems.First();
            var existingItem = items[existingItemIndex];

            // If item matches try stack
            if (existingItem.Data == item.Data)
            {
                if (StackItem(existingItem, item)) return (ItemPlaceResponse.Stacked, null);
            }

            // If have not stacked at this point replace
            RemoveItem(existingItemIndex);
            PlaceItem(item, x, y);
            return (ItemPlaceResponse.Replaced, existingItem);
        }

        // Overlapping nothing, so try place
        if (PlaceItem(item, x, y)) return (ItemPlaceResponse.Placed, item);

        return (ItemPlaceResponse.Blocked, null);
    }

    public ItemInstance TryRemoveItem(int x, int y)
    {
        if (slots[x, y] == -1) return null;
        return RemoveItem(slots[x, y]);
    }

    public bool TryRemoveItem(ItemInstance item)
    {
        int index = items.IndexOf(item);
        if (index == -1) return false;
        RemoveItem(index);
        return true;
    }

    private List<ItemInstance> items;
    private int[,] slots;
    private int sizeX;
    private int sizeY;

    private bool PlaceItem(ItemInstance item, int x, int y)
    {
        // Brute force check if the item fits in the inventory
        for (int i = 0; i < item.Data.SizeX; i++)
        {
            for (int j = 0; j < item.Data.SizeY; j++)
            {
                if (x + i >= sizeX || y + j >= sizeY || slots[x + i, y + j] != -1) return false;
            }
        }

        for (int i = 0; i < item.Data.SizeX; i++)
        {
            for (int j = 0; j < item.Data.SizeY; j++)
            {
                slots[x + i, y + j] = items.Count;
            }
        }

        items.Add(item);
        item.SetInventory(this);
        OnItemAdded?.Invoke(item, x, y);
        return true;
    }

    private bool StackItem(ItemInstance existingItem, ItemInstance item)
    {
        if (existingItem.Amount + item.Amount <= existingItem.Data.MaxStackSize)
        {
            existingItem.SetAmount(existingItem.Amount + item.Amount);
            return true;
        }
        else if (existingItem.Amount < existingItem.Data.MaxStackSize)
        {
            item.SetAmount(item.Amount - (existingItem.Data.MaxStackSize - existingItem.Amount));
            existingItem.SetAmount(existingItem.Data.MaxStackSize);
            return true;
        }

        return false;
    }

    private ItemInstance RemoveItem(int itemIndex)
    {
        ItemInstance item = items[itemIndex];
        item.SetInventory(null);
        items.RemoveAt(itemIndex);

        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++)
            {
                if (slots[x, y] == itemIndex) slots[x, y] = -1;
                else if (slots[x, y] > itemIndex) slots[x, y]--;
            }
        }

        OnItemRemoved?.Invoke(item);

        return item;
    }
}

// --- Serialization ---

public struct InventoryData
{
    public ItemInstanceData[] items;
    public int[,] slots;

    public InventoryData(ItemInstance[] items, int[,] slots)
    {
        this.items = new ItemInstanceData[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            this.items[i] = items[i].SaveToData();
        }
        this.slots = slots;
    }
}

public partial class Inventory
{
    public InventoryData SaveToData() => new(items.ToArray(), slots);

    public void LoadFromData(InventoryData data)
    {
        slots = data.slots;
        sizeX = data.slots.GetLength(0);
        sizeY = data.slots.GetLength(1);

        items.Clear();
        for (int i = 0; i < data.items.Length; i++)
        {
            items.Add(new(data.items[i].itemID, data.items[i].amount));
            items[^1].SetInventory(this);
        }
    }
}
