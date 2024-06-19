using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;

public class GridData : MonoBehaviour
{
    Dictionary<Vector3Int, PlacementData> allPositions = new();
    public Dictionary<Vector3Int, PlacementData> onlyOriginPosition = new();
    public Dictionary<Agent, AgentPlacementData> agentPositions = new();

    [SerializeField]
    Grid grid;

    public void AddObjectAt(Vector3Int gridPosition,
                            int rotationDegree,
                            PlaceableObject placeableObject,
                            EssentialPlacementData savedData = null)
    {
        Vector2Int objectSize = placeableObject.Size;
        Dictionary<int, Vector3Int> rotationPositionOffset = new Dictionary<int, Vector3Int>()
        {
            { 0, new Vector3Int(0, 0, 0) },
            { 90, new Vector3Int(0, 0, objectSize.x) },
            { 180, new Vector3Int(objectSize.x, 0, objectSize.y) },
            { 270, new Vector3Int(objectSize.y, 0, 0) }
        };
        if (savedData == null)
            placeableObject.Inventory = new Inventory(placeableObject.InventorySize);
        else
        {
            placeableObject.Inventory = new Inventory(placeableObject.InventorySize);
            for (int i = 0; i < placeableObject.InventorySize; i++)
            {
                if (savedData.InventoryItemIDs[i] != -1)
                    placeableObject.Inventory.InventorySlots[i].Item =
                        Inventory.AllItems[savedData.InventoryItemIDs[i]];
                else
                    placeableObject.Inventory.InventorySlots[i].Item = null;
                placeableObject.Inventory.InventorySlots[i].Amount = savedData.InventoryItemAmounts[i];
            }
        }
        
        GameObject newObject = Instantiate(placeableObject.Prefab);
        newObject.transform.position = grid.CellToWorld(gridPosition + rotationPositionOffset[rotationDegree]);
        newObject.transform.Rotate(0, rotationDegree,0);
        
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, placeableObject.Size, rotationDegree);
        PlacementData data = new PlacementData(positionToOccupy, placeableObject, newObject,gridPosition, rotationDegree);
        onlyOriginPosition[gridPosition] = data;
        foreach (var pos in positionToOccupy)
        {
            if (allPositions.ContainsKey(pos))
                throw new Exception($"Dictionary already contains this cell position {pos}");
            allPositions[pos] = data;
        }
    }

    List<Vector3Int> CalculatePositions(Vector3Int gridPosition, Vector2Int objectSize, int rotationDegree)
    {
        List<Vector3Int> returnVal = new();
        if (rotationDegree % 180 == 0)
        {
            for (int x = 0; x < objectSize.x; x++)
            {
                for (int y = 0; y < objectSize.y; y++)
                {
                    returnVal.Add(gridPosition + new Vector3Int(x, 0, y));
                }
            }
        }
        else
        {
            for (int x = 0; x < objectSize.x; x++)
            {
                for (int y = 0; y < objectSize.y; y++)
                {
                    returnVal.Add(gridPosition + new Vector3Int(y, 0, x));
                }
            }
        }
        return returnVal;
    }

    public bool CanPlaceObejctAt(Vector3Int gridPosition, Vector2Int objectSize, int rotationDegree)
    {
        List<Vector3Int> positionToOccupy = CalculatePositions(gridPosition, objectSize, rotationDegree);
        foreach (var pos in positionToOccupy)
        {
            if (allPositions.ContainsKey(pos))
                return false;
        }
        return true;
    }

    internal void RemoveObjectAt(Vector3Int gridPosition)
    {
        Destroy(onlyOriginPosition[gridPosition].GameObject);
        onlyOriginPosition.Remove(allPositions[gridPosition].GridPosition);
        foreach (var pos in allPositions[gridPosition].OccupiedPositions)
        {
            allPositions.Remove(pos);
        }
    }

    Vector3Int GetAdjuncentFreeSpace(Vector3Int gridPosition)
    {
        List<Vector3Int> possiblePositions = new List<Vector3Int>
        {
            new Vector3Int(gridPosition.x + 1, gridPosition.y, gridPosition.z),
            new Vector3Int(gridPosition.x - 1, gridPosition.y, gridPosition.z),
            new Vector3Int(gridPosition.x, gridPosition.y, gridPosition.z + 1),
            new Vector3Int(gridPosition.x, gridPosition.y, gridPosition.z - 1)
        };

        foreach (var candidate in possiblePositions)
        {
            if (!allPositions.ContainsKey(candidate)) // Check if position is not occupied
            {
                return candidate; // Return the first free adjacent space
            }
        }
        return new Vector3Int(-1, -1, -1);
    }

    public Vector3Int GetClosestFreeSpace(Vector3Int gridPosition)
    {
        Vector3Int rezult = GetAdjuncentFreeSpace(gridPosition);
        if (rezult.x != -1)
            return rezult;
        else {
            List<Vector3Int> occupiedPositions = allPositions[gridPosition].OccupiedPositions;
            foreach (var pos in occupiedPositions)
            {
                rezult = GetAdjuncentFreeSpace(pos);
                if (rezult.x != -1)
                    return rezult;
            }
        }
        return new Vector3Int(-1, -1, -1);
    }
    
    public PlacementData GetPlacementDataByName(string name)
    {
        Dictionary<string, int> nameCount = new Dictionary<string, int>();

        foreach (var entry in onlyOriginPosition)
        {
            var placeableObject = entry.Value.PlaceableObject;
            if (placeableObject == null)
                continue;

            string baseName = placeableObject.Name;
            if (!nameCount.ContainsKey(baseName))
            {
                nameCount[baseName] = 0;
            }
            else
            {
                nameCount[baseName]++;
                baseName += " " + nameCount[baseName].ToString();
            }
            if (baseName.ToLower() == name.ToLower())
                return entry.Value;
        }

        return null;
    }
}

public class PlacementData
{
    public List<Vector3Int> OccupiedPositions;
    public PlaceableObject PlaceableObject { get; private set; }
    public GameObject GameObject { get; private set; }
    public Vector3Int GridPosition { get; private set; }
    public int RotationDegree { get; private set; }


    public PlacementData(List<Vector3Int> occupiedPositions,
                        PlaceableObject placeableObject,
                        GameObject gameObject,
                        Vector3Int gridPosition,
                        int rotationDegree)
    {
        OccupiedPositions = occupiedPositions;
        PlaceableObject = placeableObject;
        GameObject = gameObject;
        GridPosition = gridPosition;
        RotationDegree = rotationDegree;
    }
}

public class AgentPlacementData
{
    public Vector3Int GridPosition;
    public int RotationDegree;
    public GameObject GameObject;

    public AgentPlacementData(Vector3Int gridPosition, int rotationDegree, GameObject gameObject)
    {
        GridPosition = gridPosition;
        RotationDegree = rotationDegree;
        GameObject = gameObject;
    }
}
