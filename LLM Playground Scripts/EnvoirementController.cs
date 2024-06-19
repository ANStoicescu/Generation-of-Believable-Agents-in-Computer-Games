using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    [SerializeField]
    GridData gridData;
    [SerializeField] 
    bool showRecipes;

    public string GetAllPlaceableObjectNames()
    {
        StringBuilder result = new StringBuilder();
        Dictionary<string, int> nameCount = new Dictionary<string, int>();

        foreach (var entry in gridData.onlyOriginPosition)
        {
            var placeableObject = entry.Value.PlaceableObject;
            if (placeableObject == null)
                continue;

            string baseName = placeableObject.Name;
            if (!nameCount.ContainsKey(baseName))
                nameCount[baseName] = 0;
            else
            {
                nameCount[baseName]++;
                baseName += " " + nameCount[baseName].ToString();
            }
            result.Append($"{baseName}, ");
        }

        if (result.Length != 0)
            result.Length -= 2;
        
        return result.ToString();
    }

    string GetPlaceableObjectInventory(PlaceableObject placeableObject)
    {
        StringBuilder result = placeableObject.Inventory.NameAllItems();

        if (result.Length == 0)
            result.Append($"There are no items inside the {placeableObject.Name}  ");
        
        result.Length -= 2;

        result.AppendLine($"\nTo take something from the {placeableObject.Name} use the command: " +
                          $"\"Take quantity(optional) item_name\"" +
                          $"\nTo put something in the {placeableObject.Name} use the command:" +
                          $"\"Put quantity(optional) item_name\"");
        return result.ToString();
    }

    string GetPlaceableObjectRecipes(PlaceableObject placeableObject)
    {
        StringBuilder result = new StringBuilder();

        foreach (var recipe in placeableObject.Recipes)
        {
            result.Append("Writing the following command: \"");
            if (string.IsNullOrEmpty(recipe.Action))
                result.Append("Use  ");
            else
                result.Append(recipe.Action);
            foreach (var item in recipe.RequiredItems)
            {
                if (item.amount > 1 && string.IsNullOrEmpty(item.item.PluralName))
                    result.Append($"{item.amount} {item.item.PluralName}, ");
                else
                    result.Append($"{item.amount} {item.item.Name}, ");
            }
            result.Remove(result.Length - 2, 2);
            
            result.Append("\" will result in the following items: ");
            foreach (var item in recipe.ResultingItems)
            {
                if (item.amount > 1 && string.IsNullOrEmpty(item.item.PluralName))
                    result.Append($"{item.amount} {item.item.PluralName}, ");
                else
                    result.Append($"{item.amount} {item.item.Name}, ");
            }
            result.Remove(result.Length - 2, 2);
            result.Append("\n");
        }
        
        return result.ToString();
    }

    string GetAllAgentNames()
    {
        StringBuilder result = new StringBuilder();

        foreach (var entry in gridData.agentPositions)
        {
            Agent agent = entry.Key;
            result.Append($"{agent.CharacterName}, ");
        }
        
        if (result.Length > 0)
            result.Length -= 2;
        
        return result.ToString();
    }

    string GetAgentInventory(Agent currentAgent)
    {
        StringBuilder result = currentAgent.Inventory.NameAllItems();

        if (result.Length == 0)
            return "You have no items.";
        
        result.Length -= 2;
        return result.ToString();
    }

    public string GetSeenEnvironment(Agent currentAgent)
    {
        StringBuilder result = new StringBuilder();
        result.Append("In the room, there are the following objects: ");
        result.Append(GetAllPlaceableObjectNames());
        result.Append("\nIn the room, there are the following agents: ");
        result.Append(GetAllAgentNames());
        result.Append("\nIn your inventory you have the following items: ");
        result.Append(GetAgentInventory(currentAgent));
        result.AppendLine("\nTo interact with an object use the command: \"Interact with object_name\".");
        result.AppendLine("To talk to an agent use the command: \"Talk to agent_name\".");
        result.AppendLine("To give something from your inventory to another agent use the command: " +
                          "\"Give item_quantity(optional) item_name to agent_name\".");

        return result.ToString();
    }

    public string GetPlaceableObject(PlaceableObject placeableObject)
    {
        StringBuilder result = new StringBuilder();
        
        if (placeableObject.InventorySize > 0)
        {
            result.Append($"In the {placeableObject.Name}, there are the following items: ");
            result.Append(GetPlaceableObjectInventory(placeableObject));
        }

        if (placeableObject.Recipes.Count != 0)
            if (showRecipes)
                result.Append(GetPlaceableObjectRecipes(placeableObject));
            else
                result.Append($"To use items with the {placeableObject.Name} you can use the following command: " +
                              "\"Use first_item_quantity(optional) first_item_name, second_item_quantity(optional) second_item_name, and so on\"");

        if (result.Length == 0)
            return $"The {placeableObject.Name} has no functions.";
        
        return result.ToString();
    }

    public string AgentItemExchange(Agent currentAgent, Agent agentToInteract, string command)
    {
        Dictionary<Item, int> commandItems = CommandInterpretor.ExtractItemsFromResponse(command);
        
        if(agentToInteract.Inventory.CanAddItems(commandItems))
            if(currentAgent.Inventory.CanRemoveItems(commandItems))
                foreach (var entry in commandItems)
                {
                    agentToInteract.Inventory.AddItem(entry.Key.ID, entry.Value);
                    currentAgent.Inventory.RemoveItem(entry.Key.ID, entry.Value);
                }
            else
            {
                return $"You do not have all the items you requested to give to {agentToInteract.CharacterName} from your inventory: " +
                       $"{currentAgent.Inventory.NameMissingItems(commandItems)}";
            }
        else
        {
            return $"There is not enough space in {agentToInteract.CharacterName}'s inventory." +
                   " They need to free up some space before taking any more items.";
        }
        
        Debug.Log("Give command completed");
        StringBuilder obs = new StringBuilder();
        obs.Append($"You gave the following items to {agentToInteract.CharacterName}: ");
        obs.Append(Inventory.NameAllItems(commandItems));

        return obs.ToString();
    }
    
    public string InteractPlaceableObject(string response, PlacementData placementData, Agent agent)
    {
        string lowerCaseInput = response.ToLower();

        if (lowerCaseInput.Contains("take") && lowerCaseInput.Contains("put"))
            return "Both Take and Put commands were detected please use one at a time.";

        Dictionary<Item, int> commandItems = CommandInterpretor.ExtractItemsFromResponse(response);

        if (commandItems.Count == 0)
            return "Couldn't find any valid items in the provided command";
        
        if (lowerCaseInput.Contains("take"))
        {
            if(agent.Inventory.CanAddItems(commandItems))
                if( placementData.PlaceableObject.Inventory.CanRemoveItems(commandItems))
                    foreach (var entry in commandItems)
                    {
                        agent.Inventory.AddItem(entry.Key.ID, entry.Value);
                        placementData.PlaceableObject.Inventory.RemoveItem(entry.Key.ID, entry.Value);
                    }
                else
                {
                    return $"The following items are not inside the {placementData.PlaceableObject.Name}: " +
                           $"{placementData.PlaceableObject.Inventory.NameMissingItems(commandItems)}";
                }
            else
            {
                return "There is not enough space in your inventory." +
                       " You need to free up some space before taking any more items.";
            }
            
            Debug.Log("Take command completed");
            StringBuilder obs = new StringBuilder();
            obs.Append($"You took the following items from the {placementData.PlaceableObject.Name}: ");
            obs.Append(Inventory.NameAllItems(commandItems));
            
            StartCoroutine(LLMConnection.Instance.Send(obs.ToString(), "observation", agent,
                (rez) => {}));
            
            return "Stop";
        }

        if (lowerCaseInput.Contains("put"))
        {
            if(agent.Inventory.CanRemoveItems(commandItems))
                if(placementData.PlaceableObject.Inventory.CanAddItems(commandItems))
                    foreach (var entry in commandItems)
                    {
                        agent.Inventory.RemoveItem(entry.Key.ID, entry.Value);
                        placementData.PlaceableObject.Inventory.AddItem(entry.Key.ID, entry.Value);
                    }
                else
                {
                    return $"There are not enough empty slots in the {placementData.PlaceableObject.Name}." +
                           $" You need to free up some space in the {placementData.PlaceableObject.Name} before putting items here.";
                }
            else
            {
                return $"You do not have all the items you requested to put inside the {placementData.PlaceableObject.Name} from your inventory: " +
                       $"{agent.Inventory.NameMissingItems(commandItems)}";
            }
            
            Debug.Log("Put command completed");
            StringBuilder obs = new StringBuilder();
            obs.Append($"You put the following items into the {placementData.PlaceableObject.Name}: ");
            obs.Append(Inventory.NameAllItems(commandItems));
            
            StartCoroutine(LLMConnection.Instance.Send(obs.ToString(), "observation", agent,
                (rez) => {}));
            
            return "Stop";
        }

        foreach (Recipe recipe in placementData.PlaceableObject.Recipes)
        {
            if (recipe.Action == "")
                recipe.Action = "Use";
            if (lowerCaseInput.Contains(recipe.Action.ToLower()))
            {
                // Check if RequiredItems match commandItems
                bool allItemsMatch = true;
                foreach (ItemQuantity requiredItem in recipe.RequiredItems)
                {
                    if (!commandItems.TryGetValue(requiredItem.item, out int commandItemAmount) || commandItemAmount != requiredItem.amount)
                    {
                        allItemsMatch = false;
                        break;
                    }
                }

                if (allItemsMatch)
                {
                    Debug.Log($"Matching recipe found for action '{recipe.Action}' with required items.");
                    if (agent.Inventory.CanRemoveItems(commandItems))
                    {
                        foreach (var requiredItem in recipe.RequiredItems)
                            agent.Inventory.RemoveItem(requiredItem.item.ID, requiredItem.amount);

                        if (agent.Inventory.CanAddItems(recipe.ResultingItems))
                        {
                            foreach (var rescultingItem in recipe.ResultingItems)
                                agent.Inventory.AddItem(rescultingItem.item.ID, rescultingItem.amount);
                            
                            StringBuilder obs = new StringBuilder();
                            obs.Append($"The recipe worked! You recieved the following items: ");
                            foreach (var item in recipe.ResultingItems)
                            {
                                if (item.amount > 1 && string.IsNullOrEmpty(item.item.PluralName))
                                    obs.Append($"{item.amount} {item.item.PluralName}, ");
                                else
                                    obs.Append($"{item.amount} {item.item.Name}, ");
                            }
                            obs.Remove(obs.Length - 2, 2);
            
                            StartCoroutine(LLMConnection.Instance.Send(obs.ToString(), "observation", agent,
                                (rez) => {}));
                            return "Stop";
                        }
                        else
                        {
                            foreach (var requiredItem in recipe.RequiredItems)
                                agent.Inventory.AddItem(requiredItem.item.ID, requiredItem.amount);
                            return "There is not enough space in your inventory." +
                                   " You need to free up some space before taking any more items.";
                        }
                    }
                    else
                    {
                        return
                            $"You do not have all the items required to complete this recipe. " +
                            $"Missing items: {agent.Inventory.NameMissingItems(commandItems)}.";
                    }
                    
                    return "Stop";
                }
            }
        }

        return "No command or recipe matches!";
    }
}

public class CommandInterpretor{
    
    public static string ExtractObjectName(string input)
    {
        var pattern = @"nteract with (the )?(?<objectName>\w+( \d+)?)(?=[\.\s,""]|$)";
        var match = Regex.Match(input, pattern, RegexOptions.IgnoreCase);

        if (match.Success)
            return match.Groups["objectName"].Value;

        return null;
    }

    public static Dictionary<Item, int> ExtractItemsFromResponse(string input)
    {
        List<Item> allItems = new List<Item>(Inventory.AllItems.Values);
        Dictionary<Item, int> foundItems = new Dictionary<Item, int>();

        // Regular expression to find quantity and ingredient pairs
        string pattern = @"(\d+)\s+([\w\s]+\w)";
        MatchCollection matches = Regex.Matches(input, pattern);

        foreach (Match match in matches)
        {
            int quantity = int.Parse(match.Groups[1].Value);
            string ingredient = match.Groups[2].Value.ToLower().Trim(); // Convert to lowercase for consistency

            Item matchedItem = allItems.Find(item => 
                item.Name.ToLower() == ingredient || 
                (item.PluralName?.ToLower() ?? "") == ingredient
            );

            if (matchedItem != null)
            {
                if (foundItems.ContainsKey(matchedItem))
                {
                    foundItems[matchedItem] += quantity;
                }
                else
                {
                    foundItems[matchedItem] = quantity;
                }
            }
        }

        // Adding any single items (not followed by a quantity) as quantity 1
        string[] words = input.Split(new [] { ' ', ',', '.', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        
        for (int i = 0; i < words.Length; i++)
        {
            string lowerWord = words[i].ToLower();
            Item matchedItem = allItems.Find(item => 
                item.Name.ToLower() == lowerWord || 
                (item.PluralName?.ToLower() ?? "") == lowerWord
            );

            if (matchedItem != null && !foundItems.ContainsKey(matchedItem))
            {
                foundItems[matchedItem] = 1;
            }

            // Check for multi-word items
            for (int j = 2; j <= 3 && i + j <= words.Length; j++)
            {
                string phrase = string.Join(" ", words.Skip(i).Take(j)).ToLower();
                matchedItem = allItems.Find(item => 
                    item.Name.ToLower() == phrase || 
                    (item.PluralName?.ToLower() ?? "") == phrase
                );

                if (matchedItem != null)
                {
                    foundItems[matchedItem] = 1;

                    i += j - 1; // Skip the next words that were part of the multi-word item
                    break;
                }
            }
        }

        return foundItems;
    }
 
}
