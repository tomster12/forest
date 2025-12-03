using UnityEngine;

public partial class PlayerStash : MonoBehaviour, ISaveable
{
    public static PlayerStash Instance { get; private set; }

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
        if (Instance != null) throw new System.Exception("PlayerStash already exists in the scene!");
        Instance = this;
        inventory = new Inventory(3, 3);
    }

    private void Start()
    {
        // Create an inventory UI for the main inventory
        GameObject inventoryUIGO = Instantiate(inventoryUIPrefab, canvasParent);
        InventoryUI inventoryUI = inventoryUIGO.GetComponent<InventoryUI>();
        inventoryUIRect = inventoryUIGO.GetComponent<RectTransform>();
        inventoryUI.SetInventory(inventory);
        inventoryUIRect.anchoredPosition = new Vector2(100, -500);
        SetEnabled(false);
    }
}

// --- Serialization ---

public partial class PlayerStash : MonoBehaviour, ISaveable
{
    public string SaveKey => "PlayerStash";

    public string SaveToString() => JsonUtility.ToJson(inventory);

    public void LoadFromString(string data) => inventory.LoadFromData(JsonUtility.FromJson<InventoryData>(data));
}
