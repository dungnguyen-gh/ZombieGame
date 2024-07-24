using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class NPCBase : MonoBehaviour, INPC
{
    public float minDistanceFromPlayer = 3f; //min distance
    public float maxDistanceFromPlayer = 6f; //max distance

    protected Transform player;
    protected NavMeshAgent navAgent;
    protected Animator animator;

    //health properties
    public float maxHealth = 100f;
    public float currentHealth;

    public bool isDead = false;

    private HealthBar healthBar;
    public string npcID;
    public string npcName;
    private HealthEntryUI healthEntryUI;

    private bool isInitialMove = true;
    private Vector3 lastPosition;
    private float stuckCheckInterval = 2f;
    private float stuckCheckTimer = 0f;
    protected virtual void Start()
    {
        if (player == null)
        {
            player = GameObject.FindObjectOfType<CharacterMovement>().transform;
        }
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (healthBar == null)
        {
            healthBar = GetComponentInChildren<HealthBar>();
            healthBar.SetMaxHealth(maxHealth);
        }

        HealthSystemManager.Instance.RegisterNPC(npcID, this);

        MoveToPlayerInitially();
        lastPosition = transform.position;
    }

    protected virtual void Update()
    {
        if (isDead) return;
        //update speed parameter for animations
        animator.SetFloat("speed", navAgent.velocity.magnitude);

        // Continuously follow the player
        //navAgent.SetDestination(player.position);
        if (isInitialMove)
        {
            if (navAgent.remainingDistance <= navAgent.stoppingDistance && !navAgent.pathPending)
            {
                isInitialMove = false;
            }
        }
        //follow and maintain distance with player
        else
        {
            MaintainDistanceFromPlayer();
        }
        CheckStucking();
    }
    protected void MaintainDistanceFromPlayer()
    {
        //get velocity vector of player rigidbody and calculate it magnitude/speed,
        //it illustrates how fast player moving
        //compare threshold to consider the player as moving
        if (player.GetComponent<Rigidbody>().velocity.magnitude > 0.1f)
        {
            //calculate direction vector pointing from police to player. 
            //normalized ensures the direction vector has a magnitude of 1
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(player.position, transform.position);
            //check if player in range
            if (distanceToPlayer > maxDistanceFromPlayer || distanceToPlayer < minDistanceFromPlayer)
            {
                //if the player exceeds the range, targetposition = player minus direction away from min 
                Vector3 targetPosition = player.position - directionToPlayer * minDistanceFromPlayer;
                navAgent.SetDestination(targetPosition);
            }
        }
        else
        {
            navAgent.SetDestination(transform.position); // Stop moving if the player is not moving
        }
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
        HealthSystemManager.Instance.UpdateNPCHealth(npcID, currentHealth);
        ServerTalker.Instance.UpdateNPCHealth(npcID, currentHealth);
        Debug.Log($"DIEEE Attack {npcID} Damage {currentHealth}");
        DamageTextManager.instance.ShowDamageText(damage, transform.position);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    protected virtual void Die()
    {
        isDead = true;
        animator.SetBool("isDead", true);
        navAgent.isStopped = true;
        HealthSystemManager.Instance.UnregisterNPC(npcID);
        ServerTalker.Instance.DeleteNPCData(npcID);
        Debug.Log($"DIEEE {npcID}");
        Destroy(gameObject, 5f);
    }
    public void SetID(string id)
    {
        npcID = id;
    }
    public void SetHealthEntryUI(HealthEntryUI healthEntry)
    {
        healthEntryUI = healthEntry;
        healthEntryUI.SetHealth(currentHealth);
    }
    private void MoveToPlayerInitially()
    {
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(player.position, transform.position);
        if (distanceToPlayer > maxDistanceFromPlayer || distanceToPlayer < minDistanceFromPlayer)
        {
            Vector3 targetPosition = player.position - directionToPlayer * Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            navAgent.SetDestination(targetPosition);
        }
        else
        {
            isInitialMove = false;
        }
    }
    public void SetInitialHealth(bool isServer, float initialHealth = 100f)
    {
        healthBar = GetComponentInChildren<HealthBar>();
        if (isServer)
        {
            healthBar.SetHealth(initialHealth);
        }
        else
        {
            healthBar.SetMaxHealth(maxHealth);
        }
    }
    private void CheckStucking()
    {
        //check the time elapsed since the last frame
        stuckCheckTimer += Time.deltaTime;
        if (stuckCheckTimer >= stuckCheckInterval)
        {
            stuckCheckTimer = 0;
            if(navAgent.pathStatus == NavMeshPathStatus.PathInvalid) //if the nav mesh agent path status invalid
            {
                Debug.Log("path invalid " + npcID);
                MoveToPlayerInitially();
            }
            //check if the npc velocity very low and the position has not changed significantly since last check
            //sqrt is used to avoid the square root calculation
            else if (navAgent.velocity.sqrMagnitude < 0.01f && Vector3.Distance(transform.position, lastPosition) < 0.5f)
            {
                //npc far from the player, consider stuck
                if (Vector3.Distance(player.position, transform.position) > maxDistanceFromPlayer)
                {
                    Debug.Log("npc stuck " + npcID);
                    MoveToPlayerInitially();
                }
            }
            lastPosition = transform.position; //set new
        }
    }
    public abstract void Attack(Zombie targetZombie);
}
