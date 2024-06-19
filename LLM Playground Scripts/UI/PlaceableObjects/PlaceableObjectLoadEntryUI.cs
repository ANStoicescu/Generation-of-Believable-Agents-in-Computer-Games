using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PlaceableObjectLoadEntryUI
{
    Label placeableObjectName;
    VisualElement placeableObjectPhoto;

    public void SetVisualElement(VisualElement visualElement)
    {
        placeableObjectName = visualElement.Q<Label>("PlaceableObjectName");
        placeableObjectPhoto = visualElement.Q<VisualElement>("PlaceableObjectPhoto");
    }
    
    public void SetPlaceableObjectData(PlaceableObject placeableObjectData)
    {
        placeableObjectName.text = placeableObjectData.Name;
        placeableObjectPhoto.style.backgroundImage = new StyleBackground(placeableObjectData.Icon);
    }
}
