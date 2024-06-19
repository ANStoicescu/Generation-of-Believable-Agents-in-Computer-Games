using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Recipe : ScriptableObject
{
    [field: SerializeField]
    public string Action { get; set; }
    
    [field: SerializeField]
    public List<ItemQuantity> RequiredItems { get; private set; }
    
    [field: SerializeField]
    public List<ItemQuantity> ResultingItems { get; private set; }
}

[System.Serializable]
public class ItemQuantity
{
    public Item item;
    public int amount;
}