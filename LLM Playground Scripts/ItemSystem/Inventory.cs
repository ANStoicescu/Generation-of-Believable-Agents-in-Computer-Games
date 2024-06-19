using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Inventory
{
    public SlotData[] InventorySlots;
    public static Dictionary<int, Item> AllItems = null;

    void RetrieveAllItems()
    {
        AllItems = new();
        Item[] items = Resources.LoadAll<Item>("Items");

        foreach (Item item in items)
        {
            if (item != null)
            {
                AllItems[item.ID] = item;
            }
        }
    }
    
    public Inventory(int numberOfItems)
    {
        if (AllItems == null)
            RetrieveAllItems();
        
        InventorySlots = new SlotData[numberOfItems];
        for (int i = 0; i < InventorySlots.Length; i++)
        {
            InventorySlots[i] = new SlotData();
        }
    }

    public StringBuilder NameAllItems()
    {
        StringBuilder result = new StringBuilder();

        foreach (SlotData slotData in InventorySlots)
            if (slotData.Item && slotData.Amount != 0)
                if (slotData.Amount > 1 && !string.IsNullOrEmpty(slotData.Item.PluralName))
                    result.Append($"{slotData.Amount} {slotData.Item.PluralName}, ");
                else
                    result.Append($"{slotData.Amount} {slotData.Item.Name}, ");

        return result;
    }
    
    public static string NameAllItems(Dictionary<Item, int> items)
    {
        StringBuilder result = new StringBuilder();

        foreach (var item in items)
            if (item.Key && item.Value != 0)
                if (item.Value > 1 && !string.IsNullOrEmpty(item.Key.PluralName))
                    result.Append($"{item.Value} {item.Key.PluralName}, ");
                else
                    result.Append($"{item.Value} {item.Key.Name}, ");

        result.Length -= 2;

        return result.ToString();
    }

    public void AddItem(int ID, int amount)
    {
        Item itemToAdd = AllItems[ID];

        if (itemToAdd.IsStackable)
            foreach (SlotData slot in InventorySlots)
                if (slot.Item == itemToAdd)
                {
                    slot.Amount += amount;
                    return;
                }

        foreach (SlotData slot in InventorySlots)
        {
            if (slot.Amount == 0)
            {
                slot.Item = itemToAdd;
                slot.Amount = itemToAdd.IsStackable ? amount : 1;
                return;
            }
        }

        // If no slots are available for the item, you might want to handle it here
        // For example, you could throw an exception or log a message
        Debug.Log("No available slot for the item.");
    }
    
    public void RemoveItem(int ID, int amount)
    {
        Item itemToRemove = AllItems[ID];

        foreach (SlotData slot in InventorySlots)
        {
            if (slot.Item == itemToRemove)
            {
                if (itemToRemove.IsStackable)
                {
                    slot.Amount -= amount;
                    if (slot.Amount < 0)
                    {
                        slot.Amount = 0;
                        slot.Item = null;
                    }
                    return;
                }
                slot.Item = null;
                slot.Amount = 0;
                return;
            }
        }
        
        Debug.Log("Item not found in inventory.");
    }

    // Function to check how many of a specific item are in the inventory
    public int GetItemCount(int ID)
    {
        Item itemToCount = AllItems[ID];
        
        foreach (SlotData slot in InventorySlots)
            if (slot.Item == itemToCount)
                return slot.Amount;

        return 0;
    }

    // Function to check how many free slots are remaining in the inventory
    public int GetFreeSlotCount()
    {
        int count = 0;

        foreach (SlotData slot in InventorySlots)
            if (slot.Amount == 0)
                count++;

        return count;
    }

    public bool CanAddItems(Dictionary<Item, int> itemsToAdd)
    {
        int neededSpace=0;
        foreach (var entry in itemsToAdd)
        {
            Item item = entry.Key;
            if (item.IsStackable)
            {
                bool found = false;
                foreach (SlotData slot in InventorySlots)
                    if (slot.Item != null && item.ID == slot.Item.ID)
                    {
                        found = true;
                        break;
                    }
                if (!found) neededSpace++;
            }
            else
                neededSpace++;
        }
        return neededSpace <= GetFreeSlotCount();
    }
    
    public bool CanAddItems(List<ItemQuantity> itemsToAdd)
    {
        int neededSpace = 0;
        foreach (var itemQuantity in itemsToAdd)
        {
            Item item = itemQuantity.item;
            if (item.IsStackable)
            {
                bool found = false;
                foreach (SlotData slot in InventorySlots)
                    if (slot.Item != null && item.ID == slot.Item.ID)
                    {
                        found = true;
                        break;
                    }
                if (!found) neededSpace++;
            }
            else
                neededSpace++;
        }
        return neededSpace <= GetFreeSlotCount();
    }
    
    // Function to check if you can remove multiple items from the inventory
    public bool CanRemoveItems(Dictionary<Item, int> itemsToRemove)
    {
        foreach (var entry in itemsToRemove)
        {
            Item item = entry.Key;
            int amountToRemove = entry.Value;

            bool haveItem = false;
            foreach (SlotData slot in InventorySlots)
                if (slot.Item == item)
                {
                    haveItem = true;
                    if (slot.Amount < amountToRemove)
                        return false;
                }
            if (!haveItem)
                return false;
        }

        return true; // All items can be removed
    }
    
    public string NameMissingItems(Dictionary<Item, int> itemsToRemove)
    {
        StringBuilder result = new StringBuilder();

        foreach (var entry in itemsToRemove)
        {
            Item item = entry.Key;
            int amountToRemove = entry.Value;

            bool haveItem = false;
            foreach (SlotData slot in InventorySlots)
                if (slot.Item == item)
                {
                    haveItem = true;
                    if (slot.Amount < amountToRemove)
                        if (amountToRemove - slot.Amount > 1 && !string.IsNullOrEmpty(item.PluralName))
                            result.Append($"{amountToRemove - slot.Amount} {item.PluralName}, ");
                        else
                            result.Append($"{amountToRemove - slot.Amount} {item.Name}, ");
                }
            if (!haveItem)
                if (amountToRemove > 1 && !string.IsNullOrEmpty(item.PluralName))
                    result.Append($"{amountToRemove} {item.PluralName}, ");
                else
                    result.Append($"{amountToRemove} {item.Name}, ");
        }

        return result.Length > 0 ? result.ToString().TrimEnd(' ', ',') : "All items have sufficient amounts";
    }

    public void ExecuteRecipe(Recipe recipe)
    {
        foreach (var requiredItem in recipe.RequiredItems)
        {
            RemoveItem(requiredItem.item.ID, requiredItem.amount);
        }
        foreach (var resultingItem in recipe.ResultingItems)
        {
            AddItem(resultingItem.item.ID, resultingItem.amount);
        }
    }
}

public class SlotData
{
    public Item Item;
    public int Amount;

    public SlotData()
    {
        Amount = 0;
    }
}
