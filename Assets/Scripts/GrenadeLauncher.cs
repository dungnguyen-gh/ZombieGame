using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeLauncher : NPCBase
{
    [SerializeField] private float detectRadius = 20f;
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform handPoint;

    [SerializeField] private float grenadeThrowForce = 15f;
    [SerializeField] private float grenadeDamage = 50f;
    [SerializeField] private float throwingInterval = 5f;
    private float lastThrowTime;

    [SerializeField] private float throwDelay = 0.5f; 
    protected override void Start()
    {
        base.Start();
        lastThrowTime = -throwingInterval;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (isDead) return;
        if (Time.time > lastThrowTime + throwingInterval)
        {
            CheckForZombie();
        }
    }
    private void CheckForZombie()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Zombie"))
            {
                Zombie zombie = hitCollider.GetComponent<Zombie>();
                if (zombie != null && !zombie.isDead)
                {
                    //Attack(zombie);
                    animator.SetTrigger("throw");
                    lastThrowTime = Time.time;
                    StartCoroutine(DelayedThrow(zombie));
                    return;
                }
            }
        }
    }
    private IEnumerator DelayedThrow(Zombie zombie)
    {
        yield return new WaitForSeconds(throwDelay);
        Attack(zombie);
    }
    public override void Attack(Zombie targetZombie)
    {
        //animator.SetTrigger("throw");
        GameObject grenade = Instantiate(grenadePrefab, handPoint.position, handPoint.rotation); //spawn grenade at hand
        Rigidbody rb = grenade.GetComponent<Rigidbody>(); //rb for applying physics

        Vector3 targetPosition = targetZombie.transform.position; //target position
        Vector3 throwDirection = CalculateThrowDirection(handPoint.position, targetPosition, grenadeThrowForce); //calculate direction and force

        rb.AddForce(throwDirection, ForceMode.VelocityChange); //applying calculated force to rb
        grenade.GetComponent<Grenade>().Initialize(grenadeDamage); //assgin damage value

        //lastThrowTime = Time.time; //update time
    }
    private Vector3 CalculateThrowDirection(Vector3 start, Vector3 end, float throwForce)
    {
        Vector3 direction = end - start; //cal the direction from start to end
        float h = direction.y; //store the height difference bw start and end
        direction.y = 0; //ignore the heigh now to cal horizon distance
        float distance = direction.magnitude; //calc the horizon distance
        direction.y = distance; //set the elevation for 45 degree angle
        distance += h; //add height difference
        float velocity = Mathf.Sqrt(distance * Physics.gravity.magnitude / Mathf.Sin(2 * Mathf.Deg2Rad * 45)); //cal the required velocity
        return velocity * direction.normalized; //return
    }
}
