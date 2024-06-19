using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
    
public class ItemLoadUI : MonoBehaviour
{
    [SerializeField]
    GridData gridData;

    [SerializeField]
    PlacementSystem placementSystem;
    
    // UXML template for list entries
    [FormerlySerializedAs("ItemButtonTemplate")]
    [SerializeField]
    VisualTreeAsset itemButtonTemplate;
    
    // UI element references
    ListView itemListView;
    Label itemNameLabel;
    VisualElement itemIcon;

    List<Item> allItems;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        InitializeItemList(uiDocument.rootVisualElement, itemButtonTemplate);
    }

    public void InitializeItemList(VisualElement root, VisualTreeAsset itemButtonTemplate)
    {
        allItems = new List<Item>();
        allItems.AddRange(Resources.LoadAll<Item>("Items"));
        
        this.itemButtonTemplate = itemButtonTemplate;

        itemListView = root.Q<ListView>("ItemList");
        
        itemNameLabel = root.Q<Label>("ItemName");
        itemIcon = root.Q<VisualElement>("ItemPhoto");
    
        FillItemList();
        
        itemListView.selectionChanged += OnItemSelected;
    }
    
    void FillItemList()
    {
        itemListView.makeItem = () =>
        {
            var newListEntry = itemButtonTemplate.Instantiate();
            var newListEntryLogic = new ItemLoadEntryUI();
            newListEntry.userData = newListEntryLogic;
            newListEntryLogic.SetVisualElement(newListEntry);
            return newListEntry;
        };
        
        itemListView.bindItem = (item, index) =>
        {
            (item.userData as ItemLoadEntryUI)?.SetItemData(allItems[index]);
        };
        
        itemListView.itemsSource = allItems;
    }
    
    void OnItemSelected(IEnumerable<object> selectedItems)
    {
        var selectedItem = itemListView.selectedItem as Item;
        
        if (selectedItem == null)
        {
            // itemNameLabel.text = "";
            // itemIcon.style.backgroundImage = null;
    
            return;
        }

        placementSystem.StartPlacement(selectedItem.ID);
        // itemNameLabel.text = selectedItem.Name;
        // itemIcon.style.backgroundImage = new StyleBackground(selectedItem.Icon);
    }
}