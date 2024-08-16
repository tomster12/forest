using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Inventory;

public class InventoryUI : MonoBehaviour
{
    // Real pixel sizes in image
    public static float BG_IMAGE_GRID_SIZE = 80;
    public static float BG_IMAGE_BORDER_SIZE = 1;
    public static float BG_IMAGE_PPU = 150;

    // Real grid size in world units (extra 100 factor due to canvas scaling)
    public static float GRID_SIZE => BG_IMAGE_GRID_SIZE / (BG_IMAGE_PPU / 100.0f);

    public static float GRID_BORDER_SIZE => BG_IMAGE_BORDER_SIZE / (BG_IMAGE_PPU / 100.0f);

    public Inventory Inventory { get; private set; }

    public static Vector2 GetGridSizeToWorldSize(int gridSizeX, int gridSizeY)
    {
        float width = gridSizeX * (GRID_SIZE + GRID_BORDER_SIZE) - GRID_BORDER_SIZE;
        float height = gridSizeY * (GRID_SIZE + GRID_BORDER_SIZE) - GRID_BORDER_SIZE;
        return new Vector2(width, height);
    }

    public static Vector2 GetGridPosToWorldPos(int gridPosX, int gridPosY)
    {
        float x = gridPosX * (GRID_SIZE + GRID_BORDER_SIZE);
        float y = gridPosY * (GRID_SIZE + GRID_BORDER_SIZE);
        return new Vector2(x, -y);
    }

    public static Vector2Int GetWorldPosToGridPos(Vector2 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / (GRID_SIZE + GRID_BORDER_SIZE));
        int y = Mathf.FloorToInt(-worldPos.y / (GRID_SIZE + GRID_BORDER_SIZE));
        return new Vector2Int(x, -y);
    }

    public void SetInventory(Inventory newInventory)
    {
        // Clear up old inventory
        if (Inventory != null)
        {
            Inventory.OnItemAdded -= OnItemAdded;
            Inventory.OnItemRemoved -= OnItemRemoved;
        }
        foreach (Transform child in itemHolder) DestroyImmediate(child.gameObject);
        itemUIs.Clear();

        // Set new inventory and subscribe to events
        Inventory = newInventory;
        Inventory.OnItemAdded += OnItemAdded;
        Inventory.OnItemRemoved += OnItemRemoved;

        // Rescale main panel to fit the inventory size
        mainPanel.sizeDelta = GetGridSizeToWorldSize(Inventory.SizeX, Inventory.SizeY);
    }

    public Vector2Int ScreenToInventory(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(mainPanel, screenPosition, null, out Vector2 localPoint);
        return GetWorldPosToGridPos(localPoint);
    }

    [Header("References")]
    [SerializeField] private RectTransform mainPanel;
    [SerializeField] private RectTransform itemHolder;
    [SerializeField] private Image backgroundImage;

    [Header("Prefabs")]
    [SerializeField] private GameObject itemUIPrefab;

    private Dictionary<Item, ItemUI> itemUIs = new Dictionary<Item, ItemUI>();

    private void OnItemAdded(Item item, int x, int y)
    {
        // Create new inventory item UI and
        GameObject itemUIGO = Instantiate(itemUIPrefab, itemHolder);
        ItemUI itemUI = itemUIGO.GetComponent<ItemUI>();
        itemUI.SetItem(item, x, y);
        itemUIs.Add(item, itemUI);
    }

    private void OnItemRemoved(Item item)
    {
        // Remove inventory item UI
        ItemUI itemUI = itemUIs[item];
        Destroy(itemUI.gameObject);
        itemUIs.Remove(item);
    }
}
