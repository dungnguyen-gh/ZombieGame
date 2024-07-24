using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    private CharacterMovement characterMovement;
    public float damage = 60f;
    private void Start()
    {
        characterMovement = GetComponentInParent<CharacterMovement>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (characterMovement != null && characterMovement.IsAttacking())
        {
            if (other.CompareTag("Zombie"))
            {
                Zombie zombie = other.GetComponent<Zombie>();
                if (zombie != null && zombie.enabled)
                {
                    zombie.TakeDamage(damage);
                }
            }
            
        }
    }
}
