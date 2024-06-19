using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlacementSystem : MonoBehaviour
{
    [SerializeField]
    InputManager inputManager;
    [SerializeField]
    Grid grid;

    [SerializeField]
    GameObject gridVisualization;

    [SerializeField]
    AudioClip correctPlacementClip, wrongPlacementClip;
    [SerializeField]
    AudioSource source;

    [SerializeField]
    GridData gridData;

    [SerializeField]
    PreviewSystem preview;

    Vector3Int lastDetectedPosition = Vector3Int.zero;

    IBuildingState buildingState;

    [SerializeField]
    SoundFeedback soundFeedback;

    int rotationDegree;

    void Start()
    {
        gridVisualization.SetActive(false);
    }

    public void StartPlacement(int ID)
    {
        StopPlacement();
        gridVisualization.SetActive(true);
        buildingState = new PlacementState(ID,
                                           grid,
                                           preview,
                                           gridData,
                                           soundFeedback);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    public void PlaceStructure(EssentialPlacementData data)
    {
        List<PlaceableObject> placeableObjectDatabase = new List<PlaceableObject>();
        placeableObjectDatabase.AddRange(Resources.LoadAll<PlaceableObject>("PlaceableObjects"));;

        int selectedObjectIndex = placeableObjectDatabase.FindIndex(x => x.ID == data.ID);
        
        if (selectedObjectIndex > -1)
        {
            gridData.AddObjectAt(data.GridPosition,
                data.RotationDegree,
                Instantiate(placeableObjectDatabase[selectedObjectIndex]),
                data);
        }
        else
            throw new System.Exception($"No object with ID {data.ID}");
    }

    public void StartRemoving()
    {
        StopPlacement();
        gridVisualization.SetActive(true) ;
        buildingState = new RemovingState(grid, preview, gridData, soundFeedback);
        inputManager.OnClicked += PlaceStructure;
        inputManager.OnExit += StopPlacement;
    }

    void PlaceStructure()
    {
        if(inputManager.IsPointerOverUI())
        {
            return;
        }
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);

        buildingState.OnAction(gridPosition, rotationDegree);

    }

    private void StopPlacement()
    {
        soundFeedback.PlaySound(SoundType.Click);
        if (buildingState == null)
            return;
        gridVisualization.SetActive(false);
        preview.StopShowingPreview();
        inputManager.OnClicked -= PlaceStructure;
        inputManager.OnExit -= StopPlacement;
        lastDetectedPosition = Vector3Int.zero;
        buildingState = null;
    }

    private void Update()
    {
        if (buildingState == null)
            return;
        Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartRemoving();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            rotationDegree = (rotationDegree + 90) % 360;
            buildingState.UpdatePreview(gridPosition, rotationDegree);
            lastDetectedPosition = gridPosition;
        }
        if(lastDetectedPosition != gridPosition)
        {
            buildingState.UpdatePreview(gridPosition, rotationDegree);
            lastDetectedPosition = gridPosition;
        }

    }
}
