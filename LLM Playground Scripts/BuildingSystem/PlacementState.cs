using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class PlacementState : MonoBehaviour, IBuildingState
{
    private int selectedObjectIndex = -1;
    int ID;
    Grid grid;
    PreviewSystem previewSystem;
    List<PlaceableObject> placeableObjectDatabase;
    GridData gridData;
    SoundFeedback soundFeedback;
    Dictionary<int, Vector3Int> rotationPositionOffset;
    Vector2Int objectSize;

    public PlacementState(int iD,
                          Grid grid,
                          PreviewSystem previewSystem,
                          GridData gridData,
                          SoundFeedback soundFeedback)
    {
        ID = iD;
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.gridData = gridData;
        this.soundFeedback = soundFeedback;
        
        placeableObjectDatabase = new List<PlaceableObject>();
        placeableObjectDatabase.AddRange(Resources.LoadAll<PlaceableObject>("PlaceableObjects"));;

        selectedObjectIndex = placeableObjectDatabase.FindIndex(data => data.ID == ID);

        if (selectedObjectIndex > -1)
        {
            objectSize = placeableObjectDatabase[selectedObjectIndex].Size;
            rotationPositionOffset = new Dictionary<int, Vector3Int>()
            {
                { 0, new Vector3Int(0, 0, 0) },
                { 90, new Vector3Int(0, 0, objectSize.x) },
                { 180, new Vector3Int(objectSize.x, 0, objectSize.y) },
                { 270, new Vector3Int(objectSize.y, 0, 0) }
            };
            previewSystem.StartShowingPlacementPreview(
                placeableObjectDatabase[selectedObjectIndex].Prefab,
                objectSize
                );
        }
        else
            throw new System.Exception($"No object with ID {iD}");
        
    }

    public void OnAction(Vector3Int gridPosition, int rotationDegree)
    {
        bool placementValidity = gridData.CanPlaceObejctAt(gridPosition, objectSize, rotationDegree);
        if (placementValidity == false)
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
            return;
        }
        soundFeedback.PlaySound(SoundType.Place);

        gridData.AddObjectAt(gridPosition,
            rotationDegree,
            Instantiate(placeableObjectDatabase[selectedObjectIndex]));

        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), false, rotationDegree, rotationPositionOffset[rotationDegree]);
    }

    public void UpdatePreview(Vector3Int gridPosition, int rotationDegree)
    {
        bool placementValidity = gridData.CanPlaceObejctAt(gridPosition, objectSize, rotationDegree);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), placementValidity, rotationDegree, rotationPositionOffset[rotationDegree]);
    }
}

