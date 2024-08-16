using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Inventory
{
    public Inventory(int sizeX, int sizeY)
    {
        items = new List<Item>();
        SizeX = sizeX;
        SizeY = sizeY;
        slots = new int[sizeX, sizeY];

        // Initialize slots to -1
        for (int x = 0; x < sizeX; x++)
        {
            for (int y = 0; y < sizeY; y++) slots[x, y] = -1;
        }
    }

    public event Action<Item, int, int> OnItemAdded = delegate { };

    public event Action<Item> OnItemRemoved = delegate { };

    public enum ItemPlaceResponse
    { Placed, Stacked, Replaced, Blocked };

    public int SizeX { get; private set; }
    public int SizeY { get; private set; }

    public ItemPlaceResponse TryQuickStackItem(Item item)
    {
        ItemPlaceResponse response = ItemPlaceResponse.Blocked;

        // While can stack item, stack it
        while (true)
        {
            bool found = false;

            foreach (Item i in items)
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
            for (int x = 0; x < SizeX && !found; x++)
            {
                for (int y = 0; y < SizeY && !found; y++)
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

    public (ItemPlaceResponse, Item) TryPlaceItem(Item item, int x, int y)
    {
        // Check position is in bounds
        if (x < 0 || y < 0 || x + item.Data.SizeX > SizeX || y + item.Data.SizeY > SizeY)
        {
            return (ItemPlaceResponse.Blocked, null);
        }

        // Check if item under cursor matches and stack
        if (slots[x, y] != -1)
        {
            Item existingItem = items[slots[x, y]];
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
                if (x + i >= SizeX || y + j >= SizeY || slots[x + i, y + j] == -1) continue;
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

    public Item TryRemoveItem(int x, int y)
    {
        if (slots[x, y] == -1) return null;
        return RemoveItem(slots[x, y]);
    }

    public bool TryRemoveItem(Item item)
    {
        int index = items.IndexOf(item);
        if (index == -1) return false;
        RemoveItem(index);
        return true;
    }

    private List<Item> items;
    private int[,] slots;

    private bool PlaceItem(Item item, int x, int y)
    {
        // Brute force check if the item fits in the inventory
        for (int i = 0; i < item.Data.SizeX; i++)
        {
            for (int j = 0; j < item.Data.SizeY; j++)
            {
                if (x + i >= SizeX || y + j >= SizeY || slots[x + i, y + j] != -1) return false;
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
        item.Inventory = this;
        OnItemAdded?.Invoke(item, x, y);
        return true;
    }

    private bool StackItem(Item existingItem, Item item)
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

    private Item RemoveItem(int itemIndex)
    {
        Item item = items[itemIndex];
        item.Inventory = null;
        items.RemoveAt(itemIndex);

        for (int x = 0; x < SizeX; x++)
        {
            for (int y = 0; y < SizeY; y++)
            {
                if (slots[x, y] == itemIndex) slots[x, y] = -1;
                else if (slots[x, y] > itemIndex) slots[x, y]--;
            }
        }

        OnItemRemoved?.Invoke(item);

        return item;
    }
}
