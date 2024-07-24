using SimpleJSON;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }
    private Dictionary<string, List<ResourceProducer>> producersByResourceType;

    public List<TMP_Text> resourceTexts;
    private Dictionary<string, int> localResourceData = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            producersByResourceType = new Dictionary<string, List<ResourceProducer>>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        // Initialize the UI text boxes with the initial data from the server
        //UpdateResourceUI();
    }
    public bool HasEnoughResources(int rockAmount, int waterAmount, int woodAmount)
    {
        return
            GetResourceAmount("rock") >= rockAmount &&
            GetResourceAmount("water") >= waterAmount &&
            GetResourceAmount("wood") >= woodAmount;
    }
    //return amount of resource form dictionary by name, if not found name, return 0
    private int GetResourceAmount(string resourceName)
    {
        return localResourceData.ContainsKey(resourceName) ? localResourceData[resourceName] : 0;
    }
    public void ConsumeResource(int rockAmount, int waterAmount, int woodAmount)
    {
        localResourceData["rock"] -= rockAmount;
        localResourceData["water"] -= waterAmount;
        localResourceData["wood"] -= woodAmount;
        //update to ui
        UpdateResourceUI();
        ServerTalker.Instance.PostResourceData(localResourceData);
        DebugDictionary(localResourceData);
    }
    public void RegisterProducer(ResourceProducer producer)
    {
        //register for each type of resource producer
        if (!producersByResourceType.ContainsKey(producer.resourceType))
        {
            producersByResourceType[producer.resourceType] = new List<ResourceProducer>();
        }
        producersByResourceType[producer.resourceType].Add(producer);
    }
    public void NotifyProducer(string resourceType, GameObject resource)
    {
        if (producersByResourceType.ContainsKey(resourceType))
        {
            foreach (var producer in producersByResourceType[resourceType])
            {
                producer.CollectResource(resource);
            }
        }
    }
    public void CollectResource(string resourceName, int amount)
    {
        // Update local resource data
        if (localResourceData.ContainsKey(resourceName))
        {
            localResourceData[resourceName] += amount;
        }
        else
        {
            localResourceData[resourceName] = amount;
        }
        // Update the UI text boxes
        UpdateResourceUI();

        // Notify the ServerTalker to send updated data to the server
        ServerTalker.Instance.PostResourceData(localResourceData);

        //DebugDictionary(localResourceData);
    }

    void UpdateResourceUI()
    {
        if (resourceTexts.Count == 3)
        {
            resourceTexts[0].text = $"{GetResourceAmount("rock")}";
            resourceTexts[1].text = $"{GetResourceAmount("water")}";
            resourceTexts[2].text = $"{GetResourceAmount("wood")}";
        }
        //int index = 0;
        //foreach (var resource in localResourceData)
        //{
        //    if (index < resourceTexts.Count)
        //    {
        //        resourceTexts[index].text = $"{resource.Value}";
        //    }
        //    index++;
        //}
    }

    public void UpdateResourceData(JSONArray resourceArray)
    {
        for (int i = 0; i < resourceArray.Count; i++)
        {
            string resourceName = resourceArray[i]["name"];
            int quantity = resourceArray[i]["quantity"];
            localResourceData[resourceName] = quantity;
        }

        // Update the UI text boxes
        UpdateResourceUI();
    }
    private void DebugDictionary(Dictionary<string, int> dict) 
    { 
        foreach (KeyValuePair<string, int> kvp in dict)
        {
            Debug.Log($"2222key: {kvp.Key}, Value: {kvp.Value}");
        }
    }
    public void ResetResources()
    {
        localResourceData["rock"] = 0;
        localResourceData["water"] = 0;
        localResourceData["wood"] = 0;

        // Update the UI text boxes
        UpdateResourceUI();

        Debug.Log("Resources reset to default values.");
    }
}
