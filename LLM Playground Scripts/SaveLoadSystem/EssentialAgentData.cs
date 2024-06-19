using UnityEngine;

public class EssentialAgentData
{
    public string CharacterName{ get; private set; }
    public bool IsPlayer{ get; private set; }
    public Vector3Int GridPosition { get; private set; }
    public int RotationDegree { get; private set; }
    public int[] InventoryItemIDs { get; private set; }
    public int[] InventoryItemAmounts { get; private set; }

    public EssentialAgentData(string characterName,
        bool isPlayer, Vector3Int gridPosition,
        int rotationDegree, int[] inventoryItemIDs,
        int[] inventoryItemAmounts)
    {
        CharacterName = characterName;
        IsPlayer = isPlayer;
        GridPosition = gridPosition;
        RotationDegree = rotationDegree;
        InventoryItemIDs = inventoryItemIDs;
        InventoryItemAmounts = inventoryItemAmounts;
    }
}
