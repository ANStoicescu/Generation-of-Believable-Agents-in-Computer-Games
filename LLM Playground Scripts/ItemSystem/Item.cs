using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Item : ScriptableObject
{
    [field: SerializeField]
    public int ID { get; private set; }
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public string PluralName { get; private set; }
    [field: SerializeField]
    public Sprite Icon { get; private set; }
    [field: SerializeField]
    public bool IsStackable { get; private set; }
}
