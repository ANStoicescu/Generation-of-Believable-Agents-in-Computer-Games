using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class AgentLoadEntryUI
{
    Label characterName;
    VisualElement characterPhoto;

    public void SetVisualElement(VisualElement visualElement)
    {
        characterName = visualElement.Q<Label>("CharacterName");
        characterPhoto = visualElement.Q<VisualElement>("CharacterPhoto");
    }
    
    public void SetCharacterData(Agent characterData)
    {
        characterName.text = characterData.CharacterName;
        characterPhoto.style.backgroundImage = new StyleBackground(characterData.AgentIcon);
    }
}
