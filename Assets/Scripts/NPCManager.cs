using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    public class NPCDataList
    {
        public List<NPCData> npcs;
    }

    [System.Serializable]
    public class NPCData
    {
        public string NPCID;
        public string NPCName;
        public float NPCHealth;
    }
    public GameObject nursePrefab;
    public GameObject policePrefab;
    public GameObject residentPrefab;
    public GameObject flameThrowerPrefab;
    public GameObject captainPrefab;
    public GameObject cadetPrefab;

    [SerializeField] private Transform portal;
    [SerializeField] private float spawnRadius;
    public void SpawnNPCs(List<NPCData> npcDataList)
    {
        foreach (var npcData in npcDataList)
        {
            SpawnNPC(npcData);
        }
    }
    private void SpawnNPC(NPCData npcData)
    {
        GameObject npcPrefab = null;
        switch (npcData.NPCName)
        {
            case "Nurse":
                npcPrefab = nursePrefab;
                break;
            case "Police":
                npcPrefab = policePrefab;
                break;
            case "Resident":
                npcPrefab = residentPrefab;
                break;
            case "FlameThrower":
                npcPrefab = flameThrowerPrefab;
                break;
            case "Captain":
                npcPrefab = captainPrefab;
                break;
            case "Cadet":
                npcPrefab = cadetPrefab;
                break;
            default:
                Debug.LogError("Unknown NPC name: " + npcData.NPCName);
                return;
        }

        if (npcPrefab != null)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            GameObject npcObject = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);
            NPCBase npc = npcObject.GetComponent<NPCBase>();
            npc.npcID = npcData.NPCID;
            npc.npcName = npcData.NPCName;
            npc.currentHealth = npcData.NPCHealth;
            npc.SetInitialHealth(true, npcData.NPCHealth);
            //npc.maxHealth = npcData.NPCHealth;
            //HealthSystemManager.Instance.RegisterNPC(npcData.NPCID, npc);
            npcObject.SetActive(true);
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        //Vector3 randomDirection = Random.insideUnitSphere * 20;
        //randomDirection += transform.position;
        //NavMeshHit navMeshHit = new NavMeshHit();
        //NavMesh.SamplePosition(randomDirection, out navMeshHit, 20, -1);
        //return navMeshHit.position;

        Vector3 randomDirection;
        Vector3 spawnPosition;
        NavMeshHit hit;
        do
        {
            randomDirection = Random.insideUnitSphere * spawnRadius;
            randomDirection += portal.position; //find a spawn position near portal
        }
        //sample position to check if the generated position is valid on the nav mesh
        while (!NavMesh.SamplePosition(randomDirection, out hit, spawnRadius, NavMesh.AllAreas));
        //used do-while loop because validation code only run at once when the valid positon found
        //make sure it only spawn in valid position
        spawnPosition = hit.position;
        return spawnPosition;
    }
    private void OnDrawGizmos()
    {
        if (portal != null)
        {
            // Set the Gizmo color
            Gizmos.color = Color.yellow;

            // Draw a wire sphere around the portal to represent the spawn area
            Gizmos.DrawWireSphere(portal.position, spawnRadius);
        }
    }
}
