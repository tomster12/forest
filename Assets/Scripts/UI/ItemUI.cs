using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public Item Item => item;

    [Header("References")]
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Item item;
    [SerializeField] private TMPro.TextMeshProUGUI amountText;
    [SerializeField] private Image iconImage;

    public void SetItem(Item item, int x, int y)
    {
        ChangeItem(item);

        // Update position
        rectTransform.localPosition = InventoryUI.GetGridPosToWorldPos(x, y);
    }

    public void ChangeItem(Item newItem)
    {
        // Change subscription to new item
        if (item != null) item.OnAmountChanged -= OnAmountChanged;
        item = newItem;
        item.OnAmountChanged += OnAmountChanged;

        // Update gameobject name
        gameObject.name = $"Item UI ({newItem.Data.Name})";

        // Update size, icon, amount
        rectTransform.sizeDelta = InventoryUI.GetGridSizeToWorldSize(newItem.Data.SizeX, newItem.Data.SizeY);
        iconImage.sprite = newItem.Data.Icon;
        amountText.text = item.Amount.ToString();
    }

    public void OnAmountChanged()
    {
        // Update item amount
        amountText.text = item.Amount.ToString();
    }
}
