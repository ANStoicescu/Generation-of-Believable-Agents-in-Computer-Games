using System.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class AgentController : MonoBehaviour
{
    [SerializeField]
    InputManager inputManager;
    [SerializeField]
    Grid grid;
    [SerializeField]
    GridData gridData;
    [SerializeField]
    EnvironmentController environmentController;

    bool isProcessing;
    
    private IEnumerator ControlAgents()
    {
        isProcessing = true;
        foreach (var agentEntry in gridData.agentPositions)
        {
            Agent currentAgent = agentEntry.Key;
            AgentPlacementData currentAgentPlacementData = gridData.agentPositions[currentAgent];
            PlacementData currentPlaceableObjectData = null;
            string currentPrompt = "";
            bool done = false;
            string response = "";
            while (!done)
            {
                if (currentPlaceableObjectData != null)
                {
                    MoveAgent(currentPlaceableObjectData, currentAgentPlacementData);
                    string rezult;
                    
                    currentPrompt = environmentController.GetPlaceableObject(currentPlaceableObjectData.PlaceableObject);
                    if (!currentPrompt.Contains(" has no functions."))
                    {
                        yield return StartCoroutine(LLMConnection.Instance.Send(currentPrompt,
                            "act", currentAgent, (rez) => { response = rez.Response; }));
                        rezult = environmentController.InteractPlaceableObject(response, 
                            currentPlaceableObjectData, currentAgent);
                    }
                    else
                    {
                        rezult = currentPrompt;
                    }
                    if (rezult == "Stop")
                        done = true;
                    else
                        rezult = "Error: " + rezult;
                    
                    currentPrompt = rezult + '\n';
                    currentPlaceableObjectData = null;
                }
                else
                {
                    currentPrompt += environmentController.GetSeenEnvironment(currentAgent);
                    yield return StartCoroutine(LLMConnection.Instance.Send(currentPrompt, "act",
                        currentAgent,(rez) => { response = rez.Response; }));
                    response = response.ToLower();
                    if (response.Contains("interact"))
                    {
                        string placeableObjectName = CommandInterpretor.ExtractObjectName(response);
                        Debug.Log(placeableObjectName);
                        if (placeableObjectName != null)
                            currentPlaceableObjectData = gridData.GetPlacementDataByName(placeableObjectName);
                        if (placeableObjectName == null || currentPlaceableObjectData == null)
                            currentPrompt = "Error: Could not find the placeable object's name in the command.\n";
                    }
                    if (response.Contains("talk"))
                    {
                        Agent agentToInteract = FindAgentInCommand(response);
                        if (agentToInteract == null)
                        {
                            currentPrompt = "Error: Could not find any valid agent name in the provided command.";
                            continue;
                        }
                        yield return StartCoroutine(LLMConnection.Instance.Send(agentToInteract.CharacterName, "talk",
                            currentAgent,(rez) => {}));
                    }
                    if (response.Contains("give"))
                    {
                        Agent agentToInteract = FindAgentInCommand(response);
                        if (agentToInteract == null)
                        {
                            currentPrompt = "Error: Could not find any valid agent name in the provided command.";
                            continue;
                        }
                        currentPrompt = environmentController.AgentItemExchange(currentAgent, agentToInteract, response);
                        if (currentPrompt.Contains("You gave"))
                        {
                            StartCoroutine(LLMConnection.Instance.Send(currentPrompt, "observation",
                                currentAgent, (rez) => {}));
                            done = true;
                        }
                    }
                }
            }
            break;
        }
        isProcessing = false;
    }

    public Agent FindAgentInCommand(string command)
    {
        foreach (var entry in gridData.agentPositions)
            if (command.Contains(entry.Key.CharacterName))
                return entry.Key;

        return null;
    }

    public IEnumerator NewLLMAgent(Agent agent)
    {
        string response;
        yield return StartCoroutine(LLMConnection.Instance.Send(agent.Description,
            "initialize", agent, (rez) => {}));
    }
    
    public IEnumerator LoadLLMAgent(Agent agent, int saveID)
    {
        string response;
        yield return StartCoroutine(LLMConnection.Instance.Send(saveID.ToString(),
            "load", agent, (rez) => {}));
    }
    
    public IEnumerator SaveLLMAgent(Agent agent, int saveID)
    {
        string response;
        yield return StartCoroutine(LLMConnection.Instance.Send(saveID.ToString(),
            "save", agent, (rez) => {}));
    }
    
    public void MoveAgent(PlacementData placementData, AgentPlacementData currentAgentPlacementData)
    {
        Vector3Int freePosition = gridData.GetClosestFreeSpace(placementData.GridPosition);
        Vector3 freeWorldPosition = grid.CellToWorld(freePosition);
        currentAgentPlacementData.GridPosition = freePosition;
        currentAgentPlacementData.GameObject.transform.position = freeWorldPosition;// - new Vector3(0.5f, 0, 0.5f);
    }

    void Update()
    {
        // foreach (var agentEntry in gridData.agentPositions)
        //     agentEntry.Value.GameObject.transform.position += new Vector3(0.001f, 0, 0);
        // Vector3 mousePosition = inputManager.GetSelectedMapPosition();
        // Vector3Int gridPosition = grid.WorldToCell(mousePosition);
        
        if (Input.GetKeyDown(KeyCode.C) && !isProcessing)
                StartCoroutine(ControlAgents());
    }
}
