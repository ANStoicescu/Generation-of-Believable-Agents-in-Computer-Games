using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlaceableObject : ScriptableObject
{
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public string Description { get; private set; }
    [field: SerializeField]
    public Vector2Int Size { get; private set; } = Vector2Int.one;
    [field: SerializeField]
    public GameObject Prefab { get; private set; }
    [field: SerializeField]
    public Sprite Icon{ get; private set; }

    [field: SerializeField] 
    public int InventorySize;
    [field: SerializeField]
    public Inventory Inventory;
    [field: SerializeField]
    public List<Recipe> Recipes{ get; private set; }
}