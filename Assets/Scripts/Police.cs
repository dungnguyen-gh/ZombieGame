using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Police : NPCBase
{
    public float detectionRadius = 15f;
    public int rayCount = 12; //number of ray cast around
    public GameObject bulletPrefab;
    public Transform gunTip;
    public float bulletSpeed = 30f;
    public float shootingInterval = 1.5f;

    public float bulletDamage = 40f;

    private float lastShotTime;

    protected override void Start()
    {
        base.Start();
        lastShotTime = -shootingInterval; //make sure police can shoot at beginning
    }
    protected override void Update()
    {
        base.Update();
        if (isDead) return;
        //Handle shooting if any zombie are detected
        if (Time.time > lastShotTime + shootingInterval)
        {
            CheckForZombie();
        }
    }
    private void CheckForZombie()
    {
        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * (360f / rayCount);
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            RaycastHit hit;
            
            if (Physics.Raycast(transform.position, direction, out hit, detectionRadius))
            {
                if (hit.collider.CompareTag("Zombie"))
                {
                    Zombie zombie = hit.collider.GetComponent<Zombie>();
                    if (zombie != null && !zombie.isDead)
                    {
                        Attack(zombie);
                        return; //only shoot one zombie per update
                    }
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
