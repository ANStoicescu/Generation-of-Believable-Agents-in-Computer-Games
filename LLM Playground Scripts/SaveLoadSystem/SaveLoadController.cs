using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveLoadController : MonoBehaviour
{
    [SerializeField]
    GridData gridData;

    [SerializeField]
    PlacementSystem placementSystem;

    [SerializeField]
    AgentController agentController;

    List<EssentialPlacementData> serPlacementData;
    List<EssentialAgentData> serAgentData;
    List<Agent> allAgents;

    [SerializeField]
    Grid grid;

    string saveDirPath;

    void Start()
    {
        saveDirPath = Application.persistentDataPath;
        Debug.Log(saveDirPath);
        LoadGame(PersistentData.LoadSaveID);
    }

    void NewGame()
    {
        allAgents = new List<Agent>();
        allAgents.AddRange(Resources.LoadAll<Agent>("Agents"));
        int i = -2;
        foreach (Agent agent in allAgents)
        {
            agent.Inventory = new Inventory(10);
            StartCoroutine(agentController.NewLLMAgent(agent));
            GameObject newAgent = Instantiate(agent.AgentPrefab);
            gridData.agentPositions[agent] = new AgentPlacementData(new Vector3Int(0, 0, i), 0, newAgent);
            newAgent.transform.position = grid.CellToWorld(gridData.agentPositions[agent].GridPosition);
            i++;
        }
    }

    void SaveGame(int saveID)
    {
        serPlacementData = new();
        serAgentData = new();
        int[] inventoryItemIDs;
        int[] inventoryItemAmounts;
        foreach (KeyValuePair<Vector3Int, PlacementData> kvp in gridData.onlyOriginPosition)
        {
            // Vector3Int position = kvp.Key;
            PlacementData placementData = kvp.Value;
            int inventorySize = placementData.PlaceableObject.InventorySize;

            EssentialPlacementData essentialPlacementData;
            
            if (inventorySize > 0)
            {
                inventoryItemIDs = new int[inventorySize];
                inventoryItemAmounts = new int[inventorySize];
                
                for(int i = 0; i < placementData.PlaceableObject.Inventory.InventorySlots.Length; i++)
                {
                    if (placementData.PlaceableObject.Inventory.InventorySlots[i].Item != null)
                        inventoryItemIDs[i] = placementData.PlaceableObject.Inventory.InventorySlots[i].Item.ID;
                    else
                        inventoryItemIDs[i] = -1;
                    inventoryItemAmounts[i] = placementData.PlaceableObject.Inventory.InventorySlots[i].Amount;
                }

                essentialPlacementData = new EssentialPlacementData(
                    placementData.PlaceableObject.ID,
                    placementData.GridPosition,
                    placementData.RotationDegree,
                    inventoryItemIDs,
                    inventoryItemAmounts);
            }
            else
            {
                essentialPlacementData = new EssentialPlacementData(
                    placementData.PlaceableObject.ID,
                    placementData.GridPosition,
                    placementData.RotationDegree,
                    null,
                    null);
            }
            
            serPlacementData.Add(essentialPlacementData);
        }
        foreach (KeyValuePair<Agent, AgentPlacementData> kvp in gridData.agentPositions)
        {
            Agent agent = kvp.Key;
            StartCoroutine(agentController.SaveLLMAgent(agent, saveID));
            AgentPlacementData placementData = kvp.Value;
            inventoryItemIDs = new int[10];
            inventoryItemAmounts = new int[10];
            for(int i = 0; i < agent.Inventory.InventorySlots.Length; i++)
            {
                if (agent.Inventory.InventorySlots[i].Item != null)
                    inventoryItemIDs[i] = agent.Inventory.InventorySlots[i].Item.ID;
                else
                    inventoryItemIDs[i] = -1;
                inventoryItemAmounts[i] = agent.Inventory.InventorySlots[i].Amount;
            }
            EssentialAgentData essentialAgentData = new EssentialAgentData(agent.CharacterName,
                agent.isPlayer,
                placementData.GridPosition,
                placementData.RotationDegree,
                inventoryItemIDs,
                inventoryItemAmounts);
            serAgentData.Add(essentialAgentData);

        }
        
        Directory.CreateDirectory(saveDirPath);
        
        string originPositionJson = JsonConvert.SerializeObject(serPlacementData);
        File.WriteAllText(Path.Combine(saveDirPath, "originPositions.json"), originPositionJson);
        
        string agentPositionsJson = JsonConvert.SerializeObject(serAgentData);
        File.WriteAllText(Path.Combine(saveDirPath, "agentPositions.json"), agentPositionsJson);
    }

    void LoadGame(int saveID)
    {
        if (saveID == -1)
        {
            NewGame();
            return;
        }
        string originPositionJson = File.ReadAllText(Path.Combine(saveDirPath, "originPositions.json"));
        string agentPositionsJson = File.ReadAllText(Path.Combine(saveDirPath, "agentPositions.json"));
        
        serPlacementData = JsonConvert.DeserializeObject<List<EssentialPlacementData>>(originPositionJson);
        serAgentData = JsonConvert.DeserializeObject<List<EssentialAgentData>>(agentPositionsJson);

        foreach (EssentialPlacementData data in serPlacementData)
        {
            placementSystem.PlaceStructure(data);
        }
        
        allAgents = new List<Agent>();
        allAgents.AddRange(Resources.LoadAll<Agent>("Agents"));
        
        foreach (EssentialAgentData data in serAgentData)
        {
            Agent currentAgent = null;
            foreach (Agent agent in allAgents)
            {
                if (agent.CharacterName == data.CharacterName)
                    currentAgent = agent;
            }
            if (currentAgent)
            {
                currentAgent.Inventory = new Inventory(10);
                StartCoroutine(agentController.LoadLLMAgent(currentAgent, saveID));
                GameObject newAgent = Instantiate(currentAgent.AgentPrefab);
                gridData.agentPositions[currentAgent] = new AgentPlacementData(data.GridPosition, data.RotationDegree, newAgent);
                newAgent.transform.position = grid.CellToWorld(gridData.agentPositions[currentAgent].GridPosition);
            }
            else
                throw new Exception($"No agent has the name {data.CharacterName}");
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            NewGame();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveGame(1);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            PersistentData.LoadSaveID = 1;
            SceneManager.LoadScene(1);
        }
    }
}



