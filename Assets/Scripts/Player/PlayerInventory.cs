using UnityEngine;

public partial class PlayerInventory : MonoBehaviour, ISaveable
{
    public static PlayerInventory Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform canvasParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject inventoryUIPrefab;

    public Inventory inventory { get; private set; }

    private void Awake()
    {
        if (Instance != null) throw new System.Exception("PlayerInventory already exists in the scene!");
        Instance = this;
        inventory = new Inventory(4, 3);
    }

    private void Start()
    {
        // Create an inventory UI for the main inventory
        GameObject inventoryUIGO = Instantiate(inventoryUIPrefab, canvasParent);
        InventoryUI inventoryUI = inventoryUIGO.GetComponent<InventoryUI>();
        RectTransform inventoryUIRect = inventoryUIGO.GetComponent<RectTransform>();
        inventoryUI.SetInventory(inventory);
        inventoryUIRect.anchoredPosition = new Vector2(100, -100);
    }
}

// --- Serialization ---

public partial class PlayerInventory : MonoBehaviour, ISaveable
{
    public string SaveKey => "PlayerInventory";

    public string SaveToString() => JsonUtility.ToJson(inventory);

    public void LoadFromString(string data) => inventory.LoadFromData(JsonUtility.FromJson<InventoryData>(data));
}
