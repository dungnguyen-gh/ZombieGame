using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameThrower : NPCBase
{
    public GameObject firePrefab;
    public Transform gunTip;
    public float attackInterval = 4f; //interval to release fire
    public float attackRadius = 10f; //check zombie enter range
    private float lastShotTime = 0f;

    private bool isAttacking = false;
    private GameObject currentFire;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;
        if (Time.time > lastShotTime + attackInterval)
        {
            CheckForZombies();
        }
    }

    private void CheckForZombies()
    {
        bool zombieDetected = false;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Zombie"))
            {
                Zombie zombie = hitCollider.GetComponent<Zombie>();
                if (zombie != null && !zombie.isDead)
                {
                    Attack(zombie);
                    zombieDetected = true;
                    break;
                }
            }
        }
        if (!zombieDetected)
        {
            StopAttacking();
        }
    }

    public override void Attack(Zombie targetZombie)
    {
        if (!isAttacking)
        {
            isAttacking = true;
            transform.LookAt(targetZombie.transform);
            animator.SetTrigger("shoot");
            currentFire = Instantiate(firePrefab, gunTip.position, gunTip.rotation);
            currentFire.transform.parent = gunTip;
            StartCoroutine(ResetAttackCooldown());
        }
        else
        {
            transform.LookAt(targetZombie.transform);
        }   
    }
    private void StopAttacking()
    {
        if (isAttacking)
        {
            isAttacking = false;
            if (currentFire != null)
            {
                Destroy(currentFire);
                lastShotTime = Time.time;
            }
        }
    }
    private IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackInterval);
        isAttacking = false;
        lastShotTime = Time.time;
    }
}
