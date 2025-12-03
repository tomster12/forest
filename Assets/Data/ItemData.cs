using System;
using UnityEngine;

[Serializable]
[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    public string ID;
    public string Name;
    public string Description;
    public Sprite Icon;
    public int MaxStackSize;
    public int SizeX, SizeY;
}
