using UnityEngine;

public interface IBuildingState
{
    void OnAction(Vector3Int gridPosition, int rotationDegree);
    void UpdatePreview(Vector3Int gridPosition, int rotationDegree);
}