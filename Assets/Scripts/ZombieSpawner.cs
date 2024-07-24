using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] GameObject zombiePrefab;
    //[SerializeField] int amountZombie = 10;
    [SerializeField] float spawnRadius = 50f;
    [SerializeField] float spawnInterval = 5f; //time between iterations
    //[SerializeField] float spawnPeriod = 15f; //time between spawn loop
    [SerializeField] float delayBetweenCycles = 15f; //time between cycles
    //max zombie can spawn
    public int maxZombiePerSpawn = 3;
    //private int currentZombiePerSpawn = 1;

    private List<GameObject> zombieList = new List<GameObject>();

    void Start()
    {
        //StartCoroutine(PeriodicSpawn());
        StartNextSpawn(1);
    }
    public void StartNextSpawn(int zombiesPerIteration)
    {
        StartCoroutine(SpawnZombie(zombiesPerIteration));
    }
    //use for continous spawn - not in use
    //private IEnumerator PeriodicSpawn()
    //{
    //    while (true)
    //    {
    //        StartCoroutine(SpawnZombie(currentZombiePerSpawn));
    //        //increase the number of zombies per iteration
    //        if (currentZombiePerSpawn < maxZombiePerSpawn)
    //        {
    //            currentZombiePerSpawn++;
    //        }
    //        yield return new WaitForSeconds(spawnPeriod);
    //    }
    //}
    private IEnumerator SpawnZombie(int numberZombieSpawn)
    {
        yield return new WaitForSeconds(delayBetweenCycles);
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < numberZombieSpawn; j++) //zombie per iteration
            {
                Vector3 spawnPosition = GetRandomPosition();
                GameObject zombie = Instantiate(zombiePrefab, spawnPosition, Quaternion.identity);
                zombieList.Add(zombie); //add to list
                yield return new WaitForSeconds(spawnInterval);
            }
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
    // Draw the spawn radius in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
    public void ResetSpawner(int initCycle)
    {
        foreach (GameObject zombie in zombieList)
        {
            if (zombie != null)
            {
                Destroy(zombie);
            }
        }
        zombieList.Clear();
        //reset count and start 1 cycle
        StartNextSpawn(initCycle);
    }
}
