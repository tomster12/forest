using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Inventory MainInventory { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform canvasParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject inventoryUIPrefab;

    private void Awake()
    {
        // Create main inventory
        MainInventory = new Inventory(4, 3);

        // Create an inventory UI for the main inventory
        GameObject inventoryUIGO = Instantiate(inventoryUIPrefab, canvasParent);
        InventoryUI inventoryUI = inventoryUIGO.GetComponent<InventoryUI>();
        RectTransform inventoryUIRect = inventoryUIGO.GetComponent<RectTransform>();
        inventoryUI.SetInventory(MainInventory);
        inventoryUIRect.anchoredPosition = new Vector2(100, -100);
    }
}
