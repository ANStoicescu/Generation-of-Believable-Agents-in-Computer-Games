using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
    
public class AgentLoadUI : MonoBehaviour
{
    [SerializeField]
    GridData gridData;
    
    // UXML template for list entries
    [FormerlySerializedAs("AgentButtonTemplate")]
    [SerializeField]
    VisualTreeAsset agentButtonTemplate;
    
    // UI element references
    ListView characterListView;
    Label characterSpecializationLabel;
    Label characterNameLabel;
    VisualElement characterIcon;

    List<Agent> allAgents;

    void OnEnable()
    {
        var uiDocument = GetComponent<UIDocument>();
        InitializeCharacterList(uiDocument.rootVisualElement, agentButtonTemplate);
    }

    public void InitializeCharacterList(VisualElement root, VisualTreeAsset agentButtonTemplate)
    {
        allAgents = gridData.agentPositions.Keys.ToList();
        
        this.agentButtonTemplate = agentButtonTemplate;

        characterListView = root.Q<ListView>("CharacterList");
        
        characterSpecializationLabel = root.Q<Label>("CharacterIsPlayer");
        characterNameLabel = root.Q<Label>("CharacterName");
        characterIcon = root.Q<VisualElement>("CharacterPhoto");
    
        FillCharacterList();
        
        characterListView.selectionChanged += OnCharacterSelected;
    }
    
    void FillCharacterList()
    {
        characterListView.makeItem = () =>
        {
            var newListEntry = agentButtonTemplate.Instantiate();
            var newListEntryLogic = new AgentLoadEntryUI();
            newListEntry.userData = newListEntryLogic;
            newListEntryLogic.SetVisualElement(newListEntry);
            return newListEntry;
        };
        
        characterListView.bindItem = (item, index) =>
        {
            (item.userData as AgentLoadEntryUI)?.SetCharacterData(allAgents[index]);
        };
        
        characterListView.itemsSource = allAgents;
    }
    
    void OnCharacterSelected(IEnumerable<object> selectedItems)
    {
        var selectedCharacter = characterListView.selectedItem as Agent;
        
        if (selectedCharacter == null)
        {
            characterSpecializationLabel.text = "";
            characterNameLabel.text = "";
            characterIcon.style.backgroundImage = null;
    
            return;
        }

        if (selectedCharacter.isPlayer)
            characterSpecializationLabel.text = "Human";
        else
            characterSpecializationLabel.text = "Autonomous Agent";
        characterNameLabel.text = selectedCharacter.CharacterName;
        characterIcon.style.backgroundImage = new StyleBackground(selectedCharacter.AgentIcon);
    }
}