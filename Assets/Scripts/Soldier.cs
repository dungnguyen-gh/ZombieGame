using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : NPCBase
{
    public float detectRadius = 20f;
    public GameObject bulletPrefab;
    public Transform gunTip;

    public float bulletSpeed = 50f;
    public float shootingInterval = 1.5f;
    public float bulletDamage = 100f;
    private float lastShotTime;

    protected override void Start()
    {
        base.Start();
        lastShotTime = -shootingInterval;
    }

    protected override void Update()
    {
        base.Update();
        if (isDead) return;
        //Handle shooting if any zombie are detected
        if (Time.time > lastShotTime + shootingInterval)
        {
            CheckForZombies();
        }
    }
    private void CheckForZombies()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRadius);
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
        animator.SetTrigger("shoot");
        GameObject bullet = Instantiate(bulletPrefab, gunTip.position, gunTip.rotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.useGravity = false;
        Vector3 direction = (targetZombie.transform.position - gunTip.position).normalized;
        rb.velocity = direction * bulletSpeed;
        bullet.GetComponent<Bullet>().Initialize(bulletDamage);
        lastShotTime = Time.time;
    }
}
