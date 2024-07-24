using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] GameObject npcPrefab;
    public string npcName;
    [SerializeField] int amountNPC;
    public int remainingAmountNPC;
    [SerializeField] float spawnRadius = 5f;
    [SerializeField] float spawnInterval = 10f;
    public string buildingID;
    void Start()
    {
        //SetNPCAmount();
    }
    public void SpawnNPC()
    {
        StartCoroutine(CoroutineSpawnNPC());
    }
    private IEnumerator CoroutineSpawnNPC()
    {
        while (remainingAmountNPC > 0)
        {
            yield return new WaitForSeconds(spawnInterval);

            Vector3 spawnPosition = GetRandomPosition();
            GameObject npc = Instantiate(npcPrefab, spawnPosition, Quaternion.identity);

            string npcID = GenerateID();
            NPCBase npcBase = npc.GetComponent<NPCBase>();
            npcBase.SetID(npcID);
            npcBase.npcName = npcName;
            npcBase.currentHealth = npcBase.maxHealth;
            npcBase.SetInitialHealth(false);

            remainingAmountNPC--;
            Debug.Log($"Remaining {remainingAmountNPC}/{amountNPC} in {npcName}");

            // Update the server with the new remaining amount
            ServerTalker.Instance.UpdateRemainingAmountNPC(buildingID, remainingAmountNPC);
            //post npc data to the server
            ServerTalker.Instance.PostNPCData(npcBase);
        }
    }
    private Vector3 GetRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection += transform.position;
        NavMeshHit navMeshHit = new NavMeshHit();
        NavMesh.SamplePosition(randomDirection, out navMeshHit, spawnRadius, -1);
        return navMeshHit.position;
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
    private string GenerateID()
    {
        string guid = System.Guid.NewGuid().ToString();
        string NPCID = npcName + guid;
        return NPCID;
    }
    public void SetBuildingID(GameObject go)
    {
        BuildingIdentifier buildingIdentifier = go.GetComponent<BuildingIdentifier>();
        if (buildingIdentifier != null && buildingID != buildingIdentifier.prefabID)
        {
            buildingID = buildingIdentifier.prefabID;
            Debug.Log($"set building id for {go}");
        }
    }
    public void SetNPCAmountDefault()
    {
        remainingAmountNPC = amountNPC;
    }
    public void InitializeNPC(int initialRemainingAmountNPC)
    {
        remainingAmountNPC = initialRemainingAmountNPC;
    }
}
