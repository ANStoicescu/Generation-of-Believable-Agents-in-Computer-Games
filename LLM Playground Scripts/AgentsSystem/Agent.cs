using UnityEditor.SceneManagement;
using UnityEngine;

[CreateAssetMenu]
public class Agent : ScriptableObject
{
    public string CharacterName;
    [TextArea(15,20)]
    public string Description;
    public bool isPlayer = false;
    public GameObject AgentPrefab;
    public Sprite AgentIcon;
    public Inventory Inventory;
}
