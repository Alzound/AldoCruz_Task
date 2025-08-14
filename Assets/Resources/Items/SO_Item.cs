using UnityEngine;

public class SO_Item : ScriptableObject
{
    [Header("Info")]
    public string displayName = "Item";
    public string description = "This is an item.";
    public Sprite icon;
    

    [Header("Stack")]
    public ItemCategory category = ItemCategory.Misc;
    public bool stackable = true;
    [Min(1)] public int maxStack = 99;

    [Header("Usage")]
    public int actionIndex = 0; //Index use to know which action to perform when the item is used.

    public virtual bool Use(GameObject user) { return false; }
}
