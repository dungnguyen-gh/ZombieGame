using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    private Transform player;
    private NavMeshAgent navAgent;
    private Animator animator;
    private Vector3 lastPosition;
    private float speed;

    public bool isDead = false;

    public float maxHealth = 100f;
    private float currentHealth;

    //attack damage range to random
    public float minAttackDamage = 5f;
    public float maxAttackDamage = 15f;

    public float attackRadius = 2f;
    public float detectionRadius = 15f;
    public float attackInterval = 4f;
    private float lastAttackTime;
    private Transform target; //attack npc or player
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindObjectOfType<CharacterMovement>().transform;
        }
        navAgent = this.GetComponent<NavMeshAgent>();
        animator = this.GetComponent<Animator>();
        lastPosition = this.transform.position;
        navAgent.speed = Random.Range(0.5f, 1.25f);
        navAgent.SetDestination(player.position);

        currentHealth = maxHealth;
        lastAttackTime = -attackInterval;
    }

    void Update()
    {
        if (isDead)
        {
            return;
        }
        // Set destination to player if no target
        if (target == null)
        {
            navAgent.SetDestination(player.position);
        }
        // If close to the destination or moved significantly, set destination to player
        //if (navAgent.remainingDistance < 2f || (this.transform.position - lastPosition).sqrMagnitude > 10)
        //{
        //    navAgent.SetDestination(player.position);
        //}
        //Update speed animation
        speed = (this.transform.position - lastPosition).magnitude / Time.deltaTime;
        lastPosition = this.transform.position;
        animator.SetFloat("speed", speed);
        // check NPC to attack
        if (Time.time > lastAttackTime + attackInterval)
        {
            CheckForNPCs();
        }
        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);
            if (distanceToTarget <= attackRadius && Time.time >= lastAttackTime + attackInterval)
            {
                AttackTarget();
            }
            else
            {
                navAgent.SetDestination(target.position);
            }
        }
    }
    public void Die()
    {
        if (isDead)
        {
            return;
        }
        isDead = true;
        //animator.SetTrigger("dead");
        animator.SetBool("isDead", true);
        navAgent.isStopped = true;
        //this.enabled = false;
        ScoreManager.instance.AddScore(1);
        Destroy(gameObject, 5f);
    }
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        DamageTextManager.instance.ShowDamageText(damage, transform.position);
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    private void CheckForNPCs()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("NPC") || hitCollider.CompareTag("Player"))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    NPCBase npc = hitCollider.GetComponent<NPCBase>();
                    if (npc != null && !npc.isDead)
                    {
                        closestDistance = distance;
                        closestTarget = npc.transform;
                    }
                    else if (hitCollider.CompareTag("Player"))
                    {
                        closestDistance = distance;
                        closestTarget = hitCollider.transform;
                    }
                }
            }
        }
        if (closestTarget != null)
        {
            target = closestTarget;
            navAgent.SetDestination(target.position);
        }
        else
        {
            target = null;
            navAgent.SetDestination(player.position);
        }
    }
    private void AttackTarget()
    {
        if (target.CompareTag("NPC"))
        {
            NPCBase npc = target.GetComponent<NPCBase>();
            if(npc != null && !npc.isDead)
            {
                float attackDamage = Random.Range(minAttackDamage, maxAttackDamage);
                npc.TakeDamage(attackDamage);
                animator.SetTrigger("attack");
                lastAttackTime = Time.time;
            }
        }
        else if (target.CompareTag("Player"))
        {
            CharacterMovement playerScript = target.GetComponent<CharacterMovement>();
            if(playerScript != null)
            {
                float attackDamage = Random.Range(minAttackDamage, maxAttackDamage);
                playerScript.TakeDamage(attackDamage);
                animator.SetTrigger("attack");
                lastAttackTime = Time.time;
            }
        }
    }
}
