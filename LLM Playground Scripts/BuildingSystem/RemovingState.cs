using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovingState : IBuildingState
{
    Grid grid;
    PreviewSystem previewSystem;
    GridData gridData;
    SoundFeedback soundFeedback;
    Dictionary<int, Vector3Int> rotationPositionOffset = new()
    {
        { 0, new Vector3Int(0, 0, 0) },
        { 90, new Vector3Int(0, 0, 1) },
        { 180, new Vector3Int(1, 0, 1) },
        { 270, new Vector3Int(1, 0, 0) }
    };

    public RemovingState(Grid grid,
                         PreviewSystem previewSystem,
                         GridData gridData,
                         SoundFeedback soundFeedback)
    {
        this.grid = grid;
        this.previewSystem = previewSystem;
        this.gridData = gridData;
        this.soundFeedback = soundFeedback;
        previewSystem.StartShowingRemovePreview();
    }
    
    public void OnAction(Vector3Int gridPosition, int rotationDegree)
    {
        if(gridData.CanPlaceObejctAt(gridPosition,Vector2Int.one, rotationDegree) == false)
        {
            soundFeedback.PlaySound(SoundType.Remove);
            gridData.RemoveObjectAt(gridPosition);
        }
        else 
        {
            soundFeedback.PlaySound(SoundType.wrongPlacement);
        }
        
        Vector3 cellPosition = grid.CellToWorld(gridPosition);
        previewSystem.UpdatePosition(cellPosition, CheckIfSelectionIsValid(gridPosition, rotationDegree), rotationDegree, rotationPositionOffset[rotationDegree]);
    }

    private bool CheckIfSelectionIsValid(Vector3Int gridPosition, int rotationDegree)
    {
        return !gridData.CanPlaceObejctAt(gridPosition, Vector2Int.one, rotationDegree);
    }

    public void UpdatePreview(Vector3Int gridPosition, int rotationDegree)
    {
        bool validity = CheckIfSelectionIsValid(gridPosition, rotationDegree);
        previewSystem.UpdatePosition(grid.CellToWorld(gridPosition), validity, rotationDegree, rotationPositionOffset[rotationDegree]);
    }
}
