using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public enum ItemType
    {
        None,

        // Fries path
        Potato,
        ChoppedPotato,
        CookedFries,
        BurntFries,
        CharredFries,

        // Pizza path
        Dough,
        StretchDough,
        SaucedDough,
        CheesedPizza,
        PepperoniPizza,
        CookedPizza,
        BurntPizza,
        CharredPizza
    }

    [Header("Current Item")]
    public ItemType currentItem = ItemType.None;

    public bool HasItem(ItemType item)
    {
        return currentItem == item;
    }

    public void SetItem(ItemType newItem)
    {
        currentItem = newItem;
        Debug.Log("Inventory now has: " + currentItem);
    }

    public void ClearItem()
    {
        currentItem = ItemType.None;
        Debug.Log("Inventory cleared.");
    }
}