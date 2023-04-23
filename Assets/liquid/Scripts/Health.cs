using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    private float maxHealth = 100f;
    private float currentHealth = 100f;
    private float healthRegen = 0.5f;
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
    }
    
}
