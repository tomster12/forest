using System;

[Serializable]
public class Item
{
    public event Action OnAmountChanged = delegate { };

    public ItemData Data;
    public int Amount;
    public Inventory Inventory;

    public Item(ItemData data, int amount)
    {
        Data = data;
        Amount = amount;
    }

    public void SetAmount(int amount)
    {
        Amount = amount;
        OnAmountChanged?.Invoke();
    }
}
