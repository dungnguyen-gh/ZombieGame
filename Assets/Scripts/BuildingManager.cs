using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    public List<BuildingPrefab> buildingPrefabs;
    //make custom class Serializable
    [System.Serializable]
    public class BuildingPrefab
    {
        public string name;
        public GameObject prefab;
    }
    public void SpawnBuilding(string buildingName, Vector3 position, string serverID, int remainingAmountNPC)
    {
        foreach (var building in buildingPrefabs)
        {
            if (building.name == buildingName)
            {
                GameObject newBuilding = Instantiate(building.prefab, position, Quaternion.identity);
                newBuilding.GetComponent<BuildingIdentifier>().prefabID = serverID;
                NPCSpawner npcSpawner = newBuilding.GetComponent<NPCSpawner>();
                if (npcSpawner != null)
                {
                    npcSpawner.buildingID = serverID;
                    npcSpawner.InitializeNPC(remainingAmountNPC);
                    npcSpawner.SpawnNPC();
                }
                return;
            }
        }
        Debug.LogWarning("building not found" + buildingName);
    }
}
