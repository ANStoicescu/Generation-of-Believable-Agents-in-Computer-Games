using UnityEngine;

public class EssentialPlacementData
{
    public int ID { get; private set; }
    public Vector3Int GridPosition { get; private set; }
    public int RotationDegree { get; private set; }
    public int[] InventoryItemIDs { get; private set; }
    public int[] InventoryItemAmounts { get; private set; }

    public EssentialPlacementData(int iD, Vector3Int gridPosition,
        int rotationDegree, int[] inventoryItemIDs,
        int[] inventoryItemAmounts)
    {
        ID = iD;
        GridPosition = gridPosition;
        RotationDegree = rotationDegree;
        InventoryItemIDs = inventoryItemIDs;
        InventoryItemAmounts = inventoryItemAmounts;
    }
}
