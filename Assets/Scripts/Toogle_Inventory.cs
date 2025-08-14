using UnityEngine;

public class Toogle_Inventory : MonoBehaviour
{
    [SerializeField] private Canvas inventoryCanvas;

    private void Awake()
    {
        // 3er hijo = índice 2
        if (inventoryCanvas == null && transform.childCount > 2)
            inventoryCanvas = transform.GetChild(2).GetComponentInChildren<Canvas>(true);
    }

    public void ToggleInventory()
    {
        if (!inventoryCanvas) return;
        inventoryCanvas.gameObject.SetActive(!inventoryCanvas.gameObject.activeSelf);
    }
}
