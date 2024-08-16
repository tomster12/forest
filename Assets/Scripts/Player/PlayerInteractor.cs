using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class PlayerInteractor : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform heldItemContainer;
    [SerializeField] private Transform playerBody;
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Prefabs")]
    [SerializeField] private GameObject droppedItemPrefab;
    [SerializeField] private GameObject itemUIPrefab;

    [Header("Config")]
    [SerializeField] private float pickupRadius = 2.5f;

    private bool isMousePressed = false;
    private Interactable hoveredInteractable;
    private Interactable currentInteractable;
    private List<InventoryUI> hoveredInventoryUIs;
    private List<ItemUI> hoveredItemUIs;
    private InventoryUI hoveredInventoryUI => hoveredInventoryUIs.Count > 0 ? hoveredInventoryUIs[hoveredInventoryUIs.Count - 1] : null;
    private ItemUI hoveredItemUI => hoveredItemUIs.Count > 0 ? hoveredItemUIs[hoveredItemUIs.Count - 1] : null;
    private ItemUI heldItemUI;
    private Vector2 heldItemUIOffset;
    private Vector2Int heldItemUIGridOffset;

    private void Start()
    {
        // Create held item UI disabled
        heldItemUI = Instantiate(itemUIPrefab, heldItemContainer).GetComponent<ItemUI>();
        heldItemUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        isMousePressed = Input.GetMouseButtonDown(0);
        UpdateInventories();
        UpdatePickups();
        UpdateInteractables();
    }

    private void UpdateInventories()
    {
        var raycastResults = UIUtility.GetEventSystemRaycastResults();

        // Find what inventory UIs are being hovered
        hoveredInventoryUIs = new List<InventoryUI>();
        hoveredItemUIs = new List<ItemUI>();
        foreach (var result in raycastResults)
        {
            if (result.gameObject.TryGetComponent(out InventoryUI inventoryUI)) hoveredInventoryUIs.Add(inventoryUI);
            if (result.gameObject.TryGetComponent(out ItemUI itemUI)) hoveredItemUIs.Add(itemUI);
        }

        // Holding an item so update position
        if (heldItemUI.isActiveAndEnabled) UpdateHeldItemPosition();

        // Clicking with an item
        if (heldItemUI.isActiveAndEnabled && isMousePressed)
        {
            isMousePressed = false;

            // Hovering some other inventory so try place inside
            if (hoveredInventoryUIs.Count > 0)
            {
                InventoryUI inventoryUI = hoveredInventoryUI;
                var slot = inventoryUI.ScreenToInventory(Input.mousePosition);
                var response = inventoryUI.Inventory.TryPlaceItem(heldItemUI.Item, slot.x - heldItemUIGridOffset.x, -slot.y - heldItemUIGridOffset.y);

                // Item swapped, so swap held item
                if (response.Item1 == Inventory.ItemPlaceResponse.Replaced)
                {
                    // If mouse is over the swapped item then find offset
                    if (hoveredItemUIs.Count > 0 && hoveredItemUI.Item == response.Item2)
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(hoveredItemUI.transform as RectTransform, Input.mousePosition, null, out Vector2 hoveredLocalPoint);
                        heldItemUIOffset = hoveredLocalPoint;
                        heldItemUIGridOffset = InventoryUI.GetWorldPosToGridPos(hoveredLocalPoint);
                    }

                    // Otherwise centre on the first grid slot
                    else
                    {
                        heldItemUIOffset = new Vector2(InventoryUI.GRID_SIZE / 2, -InventoryUI.GRID_SIZE / 2);
                        heldItemUIGridOffset = new Vector2Int(0, 0);
                    }

                    heldItemUI.ChangeItem(response.Item2);
                    UpdateHeldItemPosition();
                }

                // Placed or stacked, so destroy held item
                else if (response.Item1 == Inventory.ItemPlaceResponse.Placed || (response.Item1 == Inventory.ItemPlaceResponse.Stacked && heldItemUI.Item.Amount == 0))
                {
                    heldItemUI.gameObject.SetActive(false);
                }

                // Blocked, so do nothing
                else if (response.Item1 == Inventory.ItemPlaceResponse.Blocked) { }
            }

            // Not overtop inventory so drop item
            else
            {
                GameObject droppedItemGO = Instantiate(droppedItemPrefab, playerBody.position, Quaternion.identity);
                DroppedItem droppedItem = droppedItemGO.GetComponent<DroppedItem>();
                droppedItem.Set(heldItemUI.Item);
                droppedItem.SetBlockedRecently();
                droppedItem.AddRandomForce(2.0f);
                heldItemUI.gameObject.SetActive(false);
            }
        }

        // Pick up hovered item
        else if (!heldItemUI.isActiveAndEnabled && hoveredItemUIs.Count > 0 && isMousePressed)
        {
            isMousePressed = false;

            // Get top hovered item
            if (hoveredItemUI.Item.Inventory.TryRemoveItem(hoveredItemUI.Item))
            {
                // Find offset from mouse to item
                RectTransformUtility.ScreenPointToLocalPointInRectangle(hoveredItemUI.transform as RectTransform, Input.mousePosition, null, out Vector2 hoveredLocalPoint);
                heldItemUIOffset = hoveredLocalPoint;
                heldItemUIGridOffset = InventoryUI.GetWorldPosToGridPos(hoveredLocalPoint);

                // Update held item to new item
                heldItemUI.gameObject.SetActive(true);
                UpdateHeldItemPosition();
                heldItemUI.ChangeItem(hoveredItemUI.Item);
            }
        }
    }

    private void UpdateHeldItemPosition()
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(heldItemContainer, Input.mousePosition, null, out Vector2 localPoint);
        heldItemUI.transform.localPosition = localPoint - heldItemUIOffset;
    }

    private void UpdatePickups()
    {
        // Find all viable nearby dropped items
        foreach (var droppedItem in DroppedItem.AllItems)
        {
            if (!droppedItem.CanPickup) continue;
            bool isNearby = Vector3.Distance(playerBody.position, droppedItem.transform.position) < pickupRadius;
            droppedItem.SetNearby(isNearby);
            if (isNearby)
            {
                // Only pickup if not blocked recently, or otherwise pressing E
                if (droppedItem.TriedPickupRecently && !Input.GetKeyDown(KeyCode.E)) continue;

                // Try quick stack item in inventory
                var response = playerInventory.MainInventory.TryQuickStackItem(droppedItem.item);
                droppedItem.SetBlockedRecently();

                // If was successful then set picked up
                bool hasPickedUp = false;
                hasPickedUp |= response == Inventory.ItemPlaceResponse.Placed;
                hasPickedUp |= response == Inventory.ItemPlaceResponse.Stacked && droppedItem.item.Amount == 0;
                if (hasPickedUp) droppedItem.SetPickedUp(playerBody);
            }
        }
    }

    private void UpdateInteractables()
    {
        // Raycast from main camera and update hovered UI
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 100))
        {
            Interactable newHoveredInteractable = hit.rigidbody?.GetComponent<Interactable>();
            if (newHoveredInteractable != hoveredInteractable)
            {
                if (hoveredInteractable != null) hoveredInteractable.Outline.enabled = false;
                hoveredInteractable = newHoveredInteractable;
                if (hoveredInteractable != null) hoveredInteractable.Outline.enabled = true;
            }
        }

        // Check if the player is pressing the interact button
        if (isMousePressed)
        {
            if (currentInteractable != null)
            {
                currentInteractable.Toggle(false);
                currentInteractable = null;
            }
            else if (hoveredInteractable != null)
            {
                currentInteractable = hoveredInteractable;
                currentInteractable.Toggle(true);
            }
        }
    }
}
