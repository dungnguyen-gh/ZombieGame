using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceProducer : MonoBehaviour
{
    public GameObject resourcePrefab;
    public float productionInterval = 8f;
    public Transform[] spawnLocations;
    public string resourceType; //type of resource produced

    public int maxStorage = 5;
    private int currentStorage = 0;

    private List<int> availableLocations; //list to track available locations
    private Dictionary<int, GameObject> currentResources; //list to track resources still available
    void Start()
    {
        availableLocations = new List<int>();
        currentResources = new Dictionary<int, GameObject>();
        //init available locations list with all spawn location indices
        for (int i = 0; i < spawnLocations.Length; i++)
        {
            availableLocations.Add(i);
        }
        //register this producer with the ResourceManager
        ResourceManager.Instance.RegisterProducer(this);
        //Produce new resource periodically
        InvokeRepeating(nameof(ProduceResource), productionInterval, productionInterval);
    }

    private void ProduceResource()
    {
        if (currentStorage < maxStorage && availableLocations.Count > 0)
        {
            //random select one location to spawn
            int randomIndex = Random.Range(0, availableLocations.Count);
            int spawnLocationIndex = availableLocations[randomIndex];
            Transform spawnLocation = spawnLocations[spawnLocationIndex];
            //spawn resource
            GameObject resource = Instantiate(resourcePrefab, spawnLocation.position, Quaternion.identity);
            currentResources.Add(spawnLocationIndex, resource);
            availableLocations.RemoveAt(randomIndex);
            currentStorage++;
            Debug.Log($"Produce {resourcePrefab.name}. current storage: {currentStorage}/{maxStorage}");
        }
        else
        {
            Debug.Log($"Resource {resourcePrefab.name} full. Cannot produce more resource.");
        }
    }
    public void CollectResource(GameObject resource)
    {
        // Find the index of the collected resource in currentResources
        //kvp: Key-value Pair
        int locationIndex = -1;
        foreach (var kvp in currentResources)
        {
            if (kvp.Value == resource)
            {
                locationIndex = kvp.Key;
                break;
            }
        }

        if (locationIndex != -1)
        {
            // Add the corresponding location back to availableLocations
            availableLocations.Add(locationIndex);
            currentResources.Remove(locationIndex);
            currentStorage--;
            Destroy(resource);
            Debug.Log($"Resource collected from location {locationIndex}. Current storage: {currentStorage}/{maxStorage}");
        }
    }
    // Optionally, to visualize the production locations in the editor
    private void OnDrawGizmosSelected()
    {
        if (spawnLocations != null)
        {
            foreach (var location in spawnLocations)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(location.position, 0.5f);
            }
        }
    }
    public int GetCurrentStorage()
    {
        return currentStorage;
    }
}
