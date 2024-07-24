using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nurse : NPCBase
{
    public float healInterval = 5f; //time between heals
    // amount of hp to heal
    public float minHealingAmount = 5f;
    public float maxHealingAmount = 10f;

    public float attackDamage = 10f;

    private float lastHealTime;

    public float attackRadius = 2f;
    private float lastAttackTime;
    private float attackInterval = 2f;
    protected override void Start()
    {
        base.Start();
        lastHealTime = -healInterval;
    }
    protected override void Update()
    {
        base.Update();
        if (isDead) return;
        //periodically healing the player
        if (Time.time > lastHealTime + healInterval)
        {
            HealPlayer();
        }
        if(Time.time > lastAttackTime + attackInterval)
        {
            CheckForZombies();
        }

    }
    private void HealPlayer()
    {
        CharacterMovement characterScript = player.GetComponent<CharacterMovement>();
        if (characterScript != null)
        {
            float healingAmount = Random.Range(minHealingAmount, maxHealingAmount);
            characterScript.Heal(healingAmount);
            //player need heal system to heal
            //player health
            lastHealTime = Time.time;
            animator.SetTrigger("heal");
        }
    }
    private void CheckForZombies()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Zombie"))
            {
                Zombie zombie = hitCollider.GetComponent<Zombie>();
                if (zombie != null && !zombie.isDead)
                {
                    Attack(zombie);
                    return;
                }
            }
        }
    }
    public override void Attack(Zombie targetZombie)
    {
        transform.LookAt(targetZombie.transform);
        animator.SetTrigger("heal");
        targetZombie.TakeDamage(attackDamage);
        lastAttackTime = Time.time;
    }
}
