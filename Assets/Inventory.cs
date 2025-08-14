using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CategoryIcon
{
    public ItemCategory category;
    public Sprite icon;
}

public class Inventory : MonoBehaviour
{
    [Header("Datos")]
    [Min(0)] public int capacity = 0;              // arranca en 0
    [SerializeField] private List<ItemStack> slots; // arranca vacía

    // SAVE: configuración de guardado/carga
    [Header("Save/Load")]
    [SerializeField] private bool autoSave = true;  // guarda tras cada cambio
    const string SAVE_KEY = "INV_V1";               // clave en PlayerPrefs

    [Header("UI")]
    [SerializeField] private RectTransform gridParent; // UI parent container. 
    [SerializeField] private SlotUI slotPrefab;        // prefab of the slot UI.

    [Header("Posición UI simple")]
    [SerializeField] private int startX = -500;
    [SerializeField] private int stepX = 150;
    [SerializeField] private int y = 0;
    private int nextX;

    private readonly List<SlotUI> slotUIs = new();

    void Awake()
    {
        //List of slots starts empty, so we initialize it here.
        slots ??= new List<ItemStack>();
        capacity = slots.Count; // 0
        nextX = startX;
    }

    // SAVE: carga al iniciar (si no hay guardado, no crea nada).
    void Start()
    {
        LoadFromPrefs();
    }

    public ItemStack Get(int index) => slots[index];

    public int TryAdd(SO_Item item, int amount)
    {
        if (!item || amount <= 0) return amount;

        //Merge or add the item to the inventory.
        if (item.stackable)
        {
            for (int i = 0; i < slots.Count && amount > 0; i++)
            {
                var s = slots[i];
                if (!s.IsEmpty && s.item == item && s.amount < item.maxStack)
                {
                    int move = Mathf.Min(item.maxStack - s.amount, amount);
                    s.amount += move; amount -= move;
                    Set(i, s);
                }
            }
        }

        // First finds an empty, if it exists, otherwise creates a new slot.
        while (amount > 0)
        {
            int idx = slots.FindIndex(s => s.IsEmpty);
            if (idx < 0)
            {
                CreateOneUISlot();     //Creates the data for the new slot and the position in the UI.
                idx = slots.Count - 1; //Recently created slot.
            }

            int put = item.stackable ? Mathf.Min(item.maxStack, amount) : 1;
            Set(idx, new ItemStack { item = item, amount = put });
            amount -= put;
        }

        return 0; // se colocó todo
    }

    public void RemoveAt(int index, int amt)
    {
        if (index < 0 || index >= slots.Count) return;

        var s = slots[index];
        if (s.IsEmpty) return;

        s.amount -= amt;

        if (s.amount <= 0)
        {
            //Removes the slot and repositions the rest
            if (index < slotUIs.Count)
            {
                var victim = slotUIs[index];
                if (victim) Destroy(victim.gameObject);
                slotUIs.RemoveAt(index);
            }

            //Erases data from the slot.
            slots.RemoveAt(index);
            capacity = slots.Count;

            //Moves to the left all the slots after the removed one.
            for (int j = index; j < slotUIs.Count; j++)
            {
                var rt = slotUIs[j].GetComponent<RectTransform>();
                var pos = rt.anchoredPosition;
                rt.anchoredPosition = new(pos.x - stepX, pos.y); // 150 units to the left - x.
                slotUIs[j].SetIndex(j);                                   //Updates index in SlotUI
            }

            //Returns the nextX to the left, so the next slot will be positioned correctly.
            nextX -= stepX;

            // SAVE: guardar tras borrar el slot
            if (autoSave) SaveToPrefs();
        }
        else
        {
            //If there's still amount left, we just update the slot.
            Set(index, s);
        }
    }


    public void UseAt(int index, GameObject user)
    {
        var s = slots[index];
        if (s.IsEmpty) return;

        if (s.item.Use(user))
            RemoveAt(index, 1);
    }

    public void MoveOrMerge(int from, int to)
    {
        if (from == to) return;
        var a = slots[from];
        var b = slots[to];
        if (a.IsEmpty) return;

        if (b.IsEmpty)
        {
            Set(to, a);
            Set(from, ItemStack.Empty);
            // SAVE: mover simple también guarda (Set ya guarda)
            return;
        }

        if (a.item == b.item && a.item.stackable)
        {
            int space = a.item.maxStack - b.amount;
            if (space > 0)
            {
                int move = Mathf.Min(space, a.amount);
                b.amount += move;
                a.amount -= move;
                Set(to, b);
                Set(from, a.amount <= 0 ? ItemStack.Empty : a);
                // SAVE: merge guarda vía Set
                return;
            }
        }

        Set(to, a);
        Set(from, b);
        // SAVE: swap guarda vía Set
    }

    //It creates a new UI slot and adds it to the data list, also moving them with the parameter of startX.
    public void CreateOneUISlot()
    {
        //Add a new empty slot to the data list
        slots.Add(ItemStack.Empty);
        capacity = slots.Count;

        //Instance the UI prefab
        var ui = Instantiate(slotPrefab, gridParent, false);
        var rt = ui.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        //If its the first slot, we start at startX
        if (slotUIs.Count == 0) nextX = startX;

        rt.anchoredPosition = new Vector2(nextX, y);
        nextX += stepX;

        int i = slots.Count - 1;
        ui.Bind(this, i);
        ui.Render(slots[i]); //Empty for now. 
        slotUIs.Add(ui);

        // SAVE: también persistimos la creación del nuevo slot vacío
        if (autoSave) SaveToPrefs();
    }

    void Set(int index, ItemStack s)
    {
        slots[index] = s;
        UpdateSlotUI(index);
        // SAVE: guarda cada cambio de slot
        if (autoSave) SaveToPrefs();
    }

    void UpdateSlotUI(int index)
    {
        if (index < 0 || index >= slotUIs.Count) return;
        slotUIs[index].Render(slots[index]);
    }

    //This is to remove the last slot UI only if it is empty
    public void RemoveLastUISlot()
    {
        if (slotUIs.Count == 0) return;
        int lastIndex = slots.Count - 1;
        if (lastIndex < 0) return;

        if (!slots[lastIndex].IsEmpty)
        {
            Debug.LogWarning("No se puede borrar el último slot porque NO está vacío.");
            return;
        }

        var last = slotUIs[slotUIs.Count - 1];
        Destroy(last.gameObject);
        slotUIs.RemoveAt(slotUIs.Count - 1);
        nextX -= stepX;

        slots.RemoveAt(lastIndex);
        capacity = slots.Count;

        // SAVE: guardar tras quitar el último slot
        if (autoSave) SaveToPrefs();
    }

    public int GetNearestIndex(Vector2 screenPos)
    {
        if (slotUIs.Count == 0) return 0;

        //Use the coordinates of the gridParent to convert screen position to local position, this also ensures the 
        //correct position of the slots in the UI.
        var canvas = gridParent.GetComponentInParent<Canvas>();
        var cam = (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(gridParent, screenPos, cam, out var local);

        //Search for the nearest slot based on the x position.
        int nearest = 0;
        float best = float.MaxValue;
        for (int i = 0; i < slotUIs.Count; i++)
        {
            var rt = slotUIs[i].GetComponent<RectTransform>();
            float d = Mathf.Abs(rt.anchoredPosition.x - local.x);
            if (d < best) { best = d; nearest = i; }
        }
        return nearest;
    }

    // ===================== SAVE / LOAD (simple por PlayerPrefs) =====================

    [Serializable] class SlotDTO { public string itemName; public int amount; } // slot por slot
    [Serializable] class InventoryDTO { public List<SlotDTO> slots = new(); }   // lista completa

    // SAVE: guardar TODOS los slots (incluidos vacíos) para conservar posiciones
    public void SaveToPrefs()
    {
        var data = new InventoryDTO();
        for (int i = 0; i < slots.Count; i++)
        {
            var s = slots[i];
            data.slots.Add(new SlotDTO
            {
                itemName = s.IsEmpty ? "" : s.item.name,
                amount = s.IsEmpty ? 0 : s.amount
            });
        }
        var json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[Inventory] Saved {slots.Count} slots to PlayerPrefs ({SAVE_KEY}), bytes: {json.Length}");
    }

    public void LoadFromPrefs()
    {
        var json = PlayerPrefs.GetString(SAVE_KEY, "");
        if (string.IsNullOrEmpty(json)) { Debug.Log("[Inventory] No save found"); return; }

        var data = JsonUtility.FromJson<InventoryDTO>(json);
        if (data == null || data.slots == null) { Debug.LogWarning("[Inventory] Corrupt save"); return; }

        ClearAllUIAndData();

        for (int i = 0; i < data.slots.Count; i++)
        {
            CreateOneUISlot();
            var dto = data.slots[i];

            if (!string.IsNullOrEmpty(dto.itemName) && dto.amount > 0)
            {
                var so = Item_DB.Find(dto.itemName); // <— nombre correcto
                if (so != null)
                {
                    slots[i] = new ItemStack { item = so, amount = dto.amount };
                    UpdateSlotUI(i);
                }
                else
                {
                    Debug.LogWarning($"[Inventory] Item '{dto.itemName}' no encontrado por ItemDB.");
                }
            }
        }
    }


    // SAVE: limpiar todo para recargar
    private void ClearAllUIAndData()
    {
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        slotUIs.Clear();
        slots.Clear();
        capacity = 0;
        nextX = startX;
    }
}
