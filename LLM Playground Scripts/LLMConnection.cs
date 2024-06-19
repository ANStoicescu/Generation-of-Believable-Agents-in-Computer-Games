using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEngine;
using UnityEngine.Networking;

public class LLMConnection : Singleton<LLMConnection>
{
    public string baseapiURL = "http://127.0.0.1:5000/agent_server/";

    public IEnumerator Send(string prompt, string type, Agent agent, System.Action<LLMResponse> callBack)
    {
        string apiURL = baseapiURL + type + $"?agentName={agent.CharacterName}";
        
        using (UnityWebRequest UWebReq = new UnityWebRequest(apiURL, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(
                JsonConvert.SerializeObject(new LLMRequest { Observation = prompt }));

            UWebReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
            UWebReq.downloadHandler = new DownloadHandlerBuffer();
            UWebReq.disposeDownloadHandlerOnDispose = true;
            UWebReq.disposeUploadHandlerOnDispose = true;
            UWebReq.disposeCertificateHandlerOnDispose = true;
            
            UWebReq.SetRequestHeader("Content-Type", "application/json");

            yield return UWebReq.SendWebRequest();

            if (UWebReq.result == UnityWebRequest.Result.ConnectionError ||
                UWebReq.result == UnityWebRequest.Result.DataProcessingError)
            {
                Debug.Log(UWebReq.error);
            }
            else
            {
                string responseText = UWebReq.downloadHandler.text;
                var response = JsonConvert.DeserializeObject<LLMResponse>(responseText);
                callBack(response);
            }
        }
    }
}

public class LLMRequest
{
    [JsonProperty(PropertyName = "observation")]
    public string Observation;
}

public class LLMResponse
{
    [JsonProperty(PropertyName = "response")]
    public string Response;
}