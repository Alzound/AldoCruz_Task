using UnityEngine;

[System.Serializable]
public struct ItemStack
{
    public SO_Item item;
    public int amount;
    public bool IsEmpty => item == null || amount <= 0;
    public static ItemStack Empty => new ItemStack { item = null, amount = 0 };
}
