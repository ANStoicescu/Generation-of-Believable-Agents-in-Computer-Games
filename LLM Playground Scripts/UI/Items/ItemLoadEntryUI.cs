using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ItemLoadEntryUI
{
    Label itemName;
    VisualElement itemPhoto;

    public void SetVisualElement(VisualElement visualElement)
    {
        itemName = visualElement.Q<Label>("ItemName");
        itemPhoto = visualElement.Q<VisualElement>("ItemPhoto");
    }
    
    public void SetItemData(Item itemData)
    {
        itemName.text = itemData.Name;
        itemPhoto.style.backgroundImage = new StyleBackground(itemData.Icon);
    }
}
