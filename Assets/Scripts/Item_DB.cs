using System.Collections.Generic;
using UnityEngine;

public class Item_DB : MonoBehaviour
{
    static Dictionary<string, SO_Item> cache;

    // Llama: var so = ItemDB.Find("HealthPotion");
    public static SO_Item Find(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        //It load all items from Resources/Items folder only once
        if (cache == null)
        {
            cache = new Dictionary<string, SO_Item>();
            var items = Resources.LoadAll<SO_Item>("Items"); //Assets/Resources/Items.
            foreach (var it in items)
            {
                if (!cache.ContainsKey(it.name))
                    cache[it.name] = it;
            }
        }

        cache.TryGetValue(name, out var so);
        return so;
    }
}
