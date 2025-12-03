using System;
using UnityEngine;

public partial class ItemInstance
{
    public event Action OnAmountChanged = delegate { };

    public ItemData Data => ItemDatabase.GetItem(itemID);
    public int Amount { get; private set; }
    public Inventory Inventory { get; private set; }

    public ItemInstance(string itemID, int amount)
    {
        this.itemID = itemID;
        this.Amount = amount;
    }

    public void SetInventory(Inventory inventory)
    {
        this.Inventory = inventory;
    }

    public void SetAmount(int amount)
    {
        this.Amount = amount;
        OnAmountChanged?.Invoke();
    }

    private string itemID;
}

// --- Serialization ---

public struct ItemInstanceData
{
    public string itemID;
    public int amount;

    public ItemInstanceData(string itemID, int amount)
    {
        this.itemID = itemID;
        this.amount = amount;
    }
}

public partial class ItemInstance
{
    public ItemInstanceData SaveToData() => new(itemID, Amount);

    public void LoadFromData(ItemInstanceData data)
    {
        itemID = data.itemID;
        Amount = data.amount;
    }
}
