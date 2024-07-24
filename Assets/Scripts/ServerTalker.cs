using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UIElements;
//using static UnityEditor.FilePathAttribute;

public class ServerTalker : MonoBehaviour
{
    public static ServerTalker Instance; // Singleton instance for easy access

    public Transform playerTransform;
    public TMP_Text userName;

    // URL for server
    private const string serverURL = "http://localhost:8000/user/awesome";
    private const string buildingURL = "http://localhost:8000/building";
    private const string npcURL = "http://localhost:8000/npcs";
    private const string resetURL = "http://localhost:8000/resetGameData";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // Make a web request to get info from the server
        //this will be a text response.
        //this will return/continue IMMEDIATELY, but the coroutine
        //will take several miliseconds to actually get a response from the server.
        StartCoroutine(GetWebData(serverURL));
        GetAllNPCs();
    }

    IEnumerator GetWebData(string url)
    {
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Something went wrong: " + www.error);
            playerTransform.GetComponent<CharacterMovement>().SetCurrentHealth(100);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);
            ProcessServerResponse(www.downloadHandler.text);
        }
    }
    public void PostCharacterPosition(Vector3 position)
    {
        // Create JSON data to send
        string jsonString = "{\"userPosition\": {\"x\": " + position.x + ", \"y\": " + position.y + ", \"z\": " + position.z + "}}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonString);

        // Create a UnityWebRequest for POST
        StartCoroutine(PostPlayerPositionCoroutine(jsonToSend));
    }
    IEnumerator PostPlayerPositionCoroutine(byte[] jsonToSend)
    {
        // Create a UnityWebRequest for POST
        using (UnityWebRequest www = new UnityWebRequest(serverURL, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("Player position sent successfully!");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }

    void ProcessServerResponse(string rawResponse)
    {
        //that text, is actually JSON info, so we need to 
        //parse that into something we can navigate
        // Parse the JSON info
        JSONNode node = JSON.Parse(rawResponse);

        // Output some info to the console
        Debug.Log("Username: " + node["username"]);
        Debug.Log("User Position: " + node["userPosition"]["x"] + ", " + node["userPosition"]["y"] 
            + ", " + node["userPosition"]["z"]);
        //PlayerData.Setbar(node["someArray"][1]["value"]); example set data for player
        // Set user name
        if (userName != null)
        {
            userName.text = "Username: " + node["username"];
        }

        // Set player position
        if (playerTransform != null)
        {
            Vector3 newPosition = new Vector3(node["userPosition"]["x"].AsFloat, node["userPosition"]["y"].AsFloat, node["userPosition"]["z"].AsFloat);
            playerTransform.position = newPosition;
        }
        //update user health
        float userHealth = node["userhealth"].AsFloat;
        CharacterMovement characterMovement = FindObjectOfType<CharacterMovement>();
        if (characterMovement != null)
        {
            characterMovement.SetCurrentHealth(userHealth);
        }

        //update the local resource data and ui to server
        // Notify ResourceManager to update its data
        ResourceManager resourceManager = FindObjectOfType<ResourceManager>();
        if (resourceManager != null)
        {
            JSONArray resourceArray = node["resourceArray"].AsArray;
            resourceManager.UpdateResourceData(resourceArray);
        }
        //spawn buildings from server data
        JSONArray buildingArray = node["buildings"].AsArray;
        foreach (JSONNode buildingNode in buildingArray)
        {
            string buildingName = buildingNode["buildingName"];
            Vector3 position = new Vector3(buildingNode["location"]["x"].AsFloat, 
                buildingNode["location"]["y"].AsFloat, buildingNode["location"]["z"].AsFloat);
            string buildingID = buildingNode["buildingID"];
            int remainingAmountNPC = buildingNode["remainingAmountNPC"].AsInt;
            SpawnBuilding(buildingName, position, buildingID, remainingAmountNPC);
        }
        int currentScore = node["currentScore"].AsInt;
        int highScore = node["highScore"].AsInt;
        ScoreManager.instance.SetScores(currentScore, highScore);
    }
    private void SpawnBuilding(string buildingName, Vector3 position, string serverID, int remainingAmountNPC)
    {
        //logic to spawn building based on its name
        //make sure you have a prefab to handle spawning
        BuildingManager buildingManager = FindObjectOfType<BuildingManager>();
        if (buildingManager != null)
        {
            buildingManager.SpawnBuilding(buildingName, position, serverID, remainingAmountNPC);
        }
    }
    public void PostResourceData(Dictionary<string, int> resourceData)
    {
        // Post resource data
        StartCoroutine(PostResourceDataCoroutine(serverURL, resourceData));
    }

    IEnumerator PostResourceDataCoroutine(string url, Dictionary<string, int> resourceData)
    {
        JSONNode jsonNode = new JSONObject();
        JSONArray jsonArray = new JSONArray();

        // Assign key and value
        foreach (var resource in resourceData)
        {
            JSONNode resourceNode = new JSONObject();
            resourceNode["name"] = resource.Key;
            resourceNode["quantity"] = resource.Value;
            jsonArray.Add(resourceNode);
        }

        jsonNode["resourceArray"] = jsonArray;
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonNode.ToString());

        // Create a UnityWebRequest for POST
        using (UnityWebRequest www = new UnityWebRequest(url, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log("Error: " + www.error);
            }
            else
            {
                Debug.Log("Resource data sent successfully");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }
    public void PostBuildingData(string buildingName, Vector3 position, string prefabID, int remainingAmountNPC)
    {
        string jsonString = "{\"buildingID\": \"" + prefabID + "\", " +
                        "\"buildingName\": \"" + buildingName + "\", " +
                        "\"location\": {\"x\": " + position.x + ", \"y\": " + position.y + ", \"z\": " + position.z + "}, " +
                        "\"remainingAmountNPC\": " + remainingAmountNPC + "}";
        byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonString);
        StartCoroutine(PostBuildingDataCoroutine(jsonToSend));
    }
    IEnumerator PostBuildingDataCoroutine(byte[] jsonToSend)
    {
        Debug.Log("Attempting to send POST request to server");
        using (UnityWebRequest www = new UnityWebRequest(buildingURL, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                Debug.Log("Response Code: " + www.responseCode);
            }
            else
            {
                Debug.Log("Building data sent successfully!");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }
    public IEnumerator CheckBuildingExists(string buildingID, System.Action<bool> callback)
    {
        string url = buildingURL + "/" + buildingID;
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
            {
                string responseText = www.downloadHandler.text;
                Debug.Log("CheckBuildingExists Response: " + responseText);
                bool exists = responseText.Trim().ToLower() == "true";
                Debug.Log("Building Exists: " + exists);
                callback(exists);
            }
            else
            {
                Debug.Log("Checking buildingID Null: Not Exist, Add New Building Data");
                callback(false);
            }
        }
    }

    public void SaveBuildingData(string buildingName, Vector3 location, string buildingID, int remainingAmountNPC)
    {
        StartCoroutine(CheckBuildingExists(buildingID, (exists) =>
        {
            if (exists)
            {
                UpdateBuildingData(buildingID, location);
                Debug.Log("Check Existing: Update Existing Building");
            }
            else
            {
                PostBuildingData(buildingName, location, buildingID, remainingAmountNPC);
                Debug.Log("Check Existing: Add New Building");
            }
        }));
    }
    public void UpdateBuildingData(string id, Vector3 position)
    {
        // Create JSON string to send
        string jsonString = "{\"buildingID\": \"" + id + "\", \"location\": {\"x\": " + position.x + ", \"y\": " + position.y + ", \"z\": " + position.z + "}}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonString);

        // Start coroutine to send the PUT request
        StartCoroutine(UpdateBuildingDataCoroutine(id, jsonToSend));

        Debug.Log($"Updating building {id} to position {position}");
    }
    IEnumerator UpdateBuildingDataCoroutine(string id, byte[] jsonToSend)
    {
        // Construct the update URL
        string updateURL = buildingURL + "/" + id;
        Debug.Log("Update URL: " + updateURL); // Debug log for URL

        using (UnityWebRequest www = UnityWebRequest.Put(updateURL, jsonToSend))
        {
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
                Debug.LogError("Response Code: " + www.responseCode); // Debug log for response code
            }
            else
            {
                Debug.Log("Building data updated successfully");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }
    public void DeleteBuildingData(string id)
    {
        StartCoroutine(DeleteBuildingDataCoroutine(id));
        Debug.Log($"Deleting building {id}");
    }
    IEnumerator DeleteBuildingDataCoroutine(string id)
    {
        string deleteURL = buildingURL + "/" + id;
        using (UnityWebRequest www = UnityWebRequest.Delete(deleteURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("Building data deleted successfully!");
            }
        }
    }
    
    //new method for updating remainingAmountNpc
    public void UpdateRemainingAmountNPC(string buildingID, int remainingAmountNPC)
    {
        StartCoroutine(UpdateRemainingAmountNPCCoroutine(buildingID, remainingAmountNPC));
    }
    private IEnumerator UpdateRemainingAmountNPCCoroutine(string buildingID, int remainingAmountNPC)
    {
        string url = $"{buildingURL}/{buildingID}/npc";
        string json = $"{{\"remainingAmountNPC\": {remainingAmountNPC}}}";
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest request = new UnityWebRequest(url, "PATCH"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully updated remainingAmountNPC on the server");
            }
            else
            {
                Debug.LogError("Error updating remainingAmountNPC: " + request.error);
            }
        }
    }
    public void PostNPCData(NPCBase npc)
    {
        string jsonString = "{\"NPCID\": \"" + npc.npcID + "\", " +
                            "\"NPCName\": \"" + npc.npcName + "\", " +
                            "\"NPCHealth\": " + npc.currentHealth + "}";
        byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonString);
        StartCoroutine(PostNPCCoroutine(jsonToSend));
    }
    private IEnumerator PostNPCCoroutine(byte[] jsonToSend)
    {
        using (UnityWebRequest www = new UnityWebRequest(npcURL, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error posting NPC data: " + www.error);
            }
            else
            {
                Debug.Log("NPC data posted successfully!");
            }
        }
    }
    public void DeleteNPCData(string npcID)
    {
        StartCoroutine(DeleteNPCCoroutine(npcID));
    }

    private IEnumerator DeleteNPCCoroutine(string npcID)
    {
        string url = npcURL + "/" + npcID;
        using (UnityWebRequest www = UnityWebRequest.Delete(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error deleting NPC data: " + www.error);
            }
            else
            {
                Debug.Log("NPC data deleted successfully!");
            }
        }
    }
    public void UpdateNPCHealth(string npcID, float newHealth)
    {
        string jsonString = $"{{\"NPCHealth\": {newHealth}}}";
        byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonString);
        StartCoroutine(UpdateNPCHHealthCoroutine(npcID, jsonToSend));
    }

    private IEnumerator UpdateNPCHHealthCoroutine(string npcID, byte[] jsonToSend)
    {
        string url = npcURL + "/" + npcID + "/NPCHealth";
        using (UnityWebRequest www = new UnityWebRequest(url, "PATCH"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error updating NPC health: " + www.error);
            }
            else
            {
                Debug.Log("NPC health updated successfully!");
            }
        }
    }
    public void GetAllNPCs()
    {
        StartCoroutine(GetAllNPCsCoroutine());
    }

    private IEnumerator GetAllNPCsCoroutine()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(npcURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error getting NPC data: " + www.error);
            }
            else
            {
                string json = www.downloadHandler.text;
                NPCManager npcManager = FindObjectOfType<NPCManager>();
                NPCManager.NPCDataList npcDataList = JsonUtility.FromJson<NPCManager.NPCDataList>(json);
                // Check if npcDataList.npcs is not null before trying to spawn NPCs
                if (npcDataList != null && npcDataList.npcs != null)
                {
                    npcManager.SpawnNPCs(npcDataList.npcs);
                }
                else
                {
                    Debug.LogWarning("Fetched NPC data is null or empty.");
                }
            }
        }
    }
    public void PostPlayerHealth(float health)
    {
        // Create JSON data to send
        string jsonString = "{\"userhealth\": " + health + "}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonString);

        // Create a UnityWebRequest for POST
        StartCoroutine(PostPlayerHealthCoroutine(jsonToSend));
    }

    IEnumerator PostPlayerHealthCoroutine(byte[] jsonToSend)
    {
        // Create a UnityWebRequest for POST
        using (UnityWebRequest www = new UnityWebRequest(serverURL, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("Player health sent successfully!");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }
    public void ResetGameData()
    {
        StartCoroutine(ResetGameDataCoroutine());
    }

    IEnumerator ResetGameDataCoroutine()
    {
        // Create a UnityWebRequest for POST to reset game data
        using (UnityWebRequest www = new UnityWebRequest(resetURL, "POST"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error resetting game data: " + www.error);
            }
            else
            {
                Debug.Log("Game data reset successfully!");
                Debug.Log("Response: " + www.downloadHandler.text);
                // Optionally process the server response if needed
            }
        }
    }
    public void PostCurrentScore(int score)
    {
        string jsonString = "{\"currentScore\": " + score + "}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonString);
        StartCoroutine(PostScoreCoroutine(jsonToSend));
    }

    public void PostHighScore(int score)
    {
        string jsonString = "{\"highScore\": " + score + "}";
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonString);
        StartCoroutine(PostScoreCoroutine(jsonToSend));
    }

    IEnumerator PostScoreCoroutine(byte[] jsonToSend)
    {
        using (UnityWebRequest www = new UnityWebRequest(serverURL, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(jsonToSend);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("Score data sent successfully!");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }
}
