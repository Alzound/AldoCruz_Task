using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Misc")]
public class Misc : SO_Item
{
    private void OnValidate()
    {
        category = ItemCategory.Misc;
        stackable = true;
        maxStack = Mathf.Max(1, maxStack);
        actionIndex = Mathf.Clamp(actionIndex, 0, 3);
    }


    public override bool Use(GameObject user)
    {
        // TODO: Integra con tu sistema (ej. PlayerStats)
        // var stats = user.GetComponent<PlayerStats>();
        // if (stats && stats.Heal(healAmount)) return true;

        // Mientras no tengas sistema, simula éxito:
        return true; // => Inventario restará 1 unidad
    }
}
