[System.Serializable]
public class Item
{
    public ItemCategory category;
    public int amount;

    public Item(ItemCategory category, int amount = 1)
    {
        this.category = category;
        this.amount = amount;
    }
}