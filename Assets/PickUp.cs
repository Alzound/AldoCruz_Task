using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickUp : MonoBehaviour
{
    public SO_Item item;
    public int amount = 1;

    void Reset() { GetComponent<Collider2D>().isTrigger = true; }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        var inv = other.GetComponentInChildren<Inventory>(); if (!inv) return;
        inv.TryAdd(item, amount);
        Destroy(gameObject);
    }
}
