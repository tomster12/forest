using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemData GetItem(string itemID)
    {
        if (instance.itemDictionary.TryGetValue(itemID, out ItemData item))
        {
            return item;
        }
        Debug.LogError($"Item with ID {itemID} not found in the database.");
        return null;
    }

    private static ItemDatabase instance;

    private readonly List<ItemData> items = new();
    private readonly Dictionary<string, ItemData> itemDictionary = new();

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < items.Count; i++)
        {
            itemDictionary.Add(items[i].ID, items[i]);
        }
    }
}
