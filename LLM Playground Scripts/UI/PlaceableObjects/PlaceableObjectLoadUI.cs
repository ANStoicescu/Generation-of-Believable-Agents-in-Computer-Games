using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
    
public class PlaceableObjectLoadUI : MonoBehaviour
{
    [SerializeField]
    GridData gridData;

    [SerializeField]
    PlacementSystem placementSystem;
    
    // UXML template for list entries
    [FormerlySerializedAs("PlaceableObjectButtonTemplate")]
    [SerializeField]
    VisualTreeAsset placeableObjectButtonTemplate;
    
    // UI element references
    ListView placeableObjectListView;
    Label placeableObjectNameLabel;
    VisualElement placeableObjectIcon;

    List<PlaceableObject> allPlaceableObjects;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        InitializePlaceableObjectList(uiDocument.rootVisualElement, placeableObjectButtonTemplate);
    }

    public void InitializePlaceableObjectList(VisualElement root, VisualTreeAsset placeableObjectButtonTemplate)
    {
        allPlaceableObjects = new List<PlaceableObject>();
        allPlaceableObjects.AddRange(Resources.LoadAll<PlaceableObject>("PlaceableObjects"));
        
        this.placeableObjectButtonTemplate = placeableObjectButtonTemplate;

        placeableObjectListView = root.Q<ListView>("PlaceableObjectList");
        
        placeableObjectNameLabel = root.Q<Label>("PlaceableObjectName");
        placeableObjectIcon = root.Q<VisualElement>("PlaceableObjectPhoto");
    
        FillPlaceableObjectList();
        
        placeableObjectListView.selectionChanged += OnPlaceableObjectSelected;
    }
    
    void FillPlaceableObjectList()
    {
        placeableObjectListView.makeItem = () =>
        {
            var newListEntry = placeableObjectButtonTemplate.Instantiate();
            var newListEntryLogic = new PlaceableObjectLoadEntryUI();
            newListEntry.userData = newListEntryLogic;
            newListEntryLogic.SetVisualElement(newListEntry);
            return newListEntry;
        };
        
        placeableObjectListView.bindItem = (item, index) =>
        {
            (item.userData as PlaceableObjectLoadEntryUI)?.SetPlaceableObjectData(allPlaceableObjects[index]);
        };
        
        placeableObjectListView.itemsSource = allPlaceableObjects;
    }
    
    void OnPlaceableObjectSelected(IEnumerable<object> selectedItems)
    {
        var selectedPlaceableObject = placeableObjectListView.selectedItem as PlaceableObject;
        
        if (selectedPlaceableObject == null)
        {
            // placeableObjectNameLabel.text = "";
            // placeableObjectIcon.style.backgroundImage = null;
    
            return;
        }

        placementSystem.StartPlacement(selectedPlaceableObject.ID);
        // placeableObjectNameLabel.text = selectedPlaceableObject.Name;
        // placeableObjectIcon.style.backgroundImage = new StyleBackground(selectedPlaceableObject.Icon);
    }
}