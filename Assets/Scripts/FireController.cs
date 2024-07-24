using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireController : MonoBehaviour
{
    private float fireDamage;
    private float fireDuration;
    private float intervalDamage = 0.5f;
    private List<Zombie> zombies = new List<Zombie>();
    private bool isDealingDamage = false;

    private void Start()
    {
        Initialize(10f, 5f);
    }
    public void Initialize(float damage, float duration)
    {
        fireDamage = damage;
        fireDuration = duration;
        Destroy(gameObject, fireDuration);
    }
    void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Zombie"))
        {
            Zombie zombie = other.GetComponent<Zombie>();
            if (zombie != null && !zombie.isDead && !zombies.Contains(zombie))
            {
                zombies.Add(zombie);
                if (!isDealingDamage)
                {
                    StartCoroutine(DealDamageOverTime());
                }
            }
        }
    }
    private IEnumerator DealDamageOverTime()
    {
        isDealingDamage = true;
        while (zombies.Count > 0)
        {
            yield return new WaitForSeconds(intervalDamage);
            foreach (Zombie zombie in new List<Zombie>(zombies))
            {
                if (zombie != null && !zombie.isDead)
                {
                    zombie.TakeDamage(fireDamage);
                }
                else
                {
                    zombies.Remove(zombie);
                }
            }
        }
        isDealingDamage = false;
    }
    private void OnDestroy()
    {
        zombies.Clear();
    }
}
