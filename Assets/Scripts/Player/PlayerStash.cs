using UnityEngine;

public class PlayerStash : MonoBehaviour
{
    public void SetEnabled(bool enabled)
    {
        inventoryUIRect.gameObject.SetActive(enabled);
    }

    [Header("References")]
    [SerializeField] private RectTransform canvasParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject inventoryUIPrefab;

    private Inventory inventory;
    private RectTransform inventoryUIRect;

    private void Awake()
    {
        // Create stash inventory
        inventory = new Inventory(3, 3);

        // Create an inventory UI for the main inventory
        GameObject inventoryUIGO = Instantiate(inventoryUIPrefab, canvasParent);
        InventoryUI inventoryUI = inventoryUIGO.GetComponent<InventoryUI>();
        inventoryUIRect = inventoryUIGO.GetComponent<RectTransform>();
        inventoryUI.SetInventory(inventory);
        inventoryUIRect.anchoredPosition = new Vector2(100, -500);
        SetEnabled(false);
    }
}
