using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class Consumable : SO_Item
{
    private void OnValidate()
    {
        category = ItemCategory.Consumable;
        stackable = true;
        maxStack = Mathf.Max(1, maxStack);
        actionIndex = Mathf.Clamp(actionIndex, 0, 2);
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
