using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public GameObject explosionEffect; //vfx prefab
    public float explosionRadius = 5f;
    public float explosionForce = 700f;
    private float grenadeDamage;
    public void Initialize(float damage)
    {
        grenadeDamage = damage;
        StartCoroutine(DelayExplosion());
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Zombie") || collision.gameObject.CompareTag("Environment"))
        {
            StopCoroutine(DelayExplosion());
            Explode();
        }
    }
    private void Explode()
    {
        GameObject explosion = Instantiate(explosionEffect, transform.position, transform.rotation);
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }
            if (collider.CompareTag("Zombie"))
            {
                Zombie zombie = collider.GetComponent<Zombie>();
                if (zombie != null && !zombie.isDead)
                {
                    zombie.TakeDamage(grenadeDamage);
                }
            }
        }
        Destroy(gameObject);
        Destroy(explosion, 2f);
    }
    private IEnumerator DelayExplosion()
    {
        yield return new WaitForSeconds(10f);
        Explode();
    }
}
