using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject droppedItemPrefab;

    [Header("Config")]
    [SerializeField] private float spawnForce = 5f;
    [SerializeField] private float spawnFrequency = 0.5f;
    [SerializeField] private List<ItemData> items;

    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnFrequency)
        {
            timer = 0f;
            SpawnItem();
        }
    }

    private void SpawnItem()
    {
        // Spawn item and get components
        GameObject droppedItemGO = Instantiate(droppedItemPrefab, transform.position, Quaternion.identity);
        DroppedItem droppedItem = droppedItemGO.GetComponent<DroppedItem>();

        // Force in random direction
        droppedItem.AddRandomForce(spawnForce);

        // Pick a random item and stack size
        ItemData itemData = items[Random.Range(0, items.Count)];
        int stackSize = Random.Range(1, itemData.MaxStackSize + 1);
        droppedItem.Set(new Item(itemData, stackSize));
    }
}
