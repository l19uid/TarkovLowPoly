using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public enum GrenadeType
    {
        Frag,
        Smoke,
        Flash,
        Impact
    }

    public GrenadeType grenadeType;
    public float fuseTime = 3f;
    public float explosionRadius = 5f;
    public float explosionForce = 100f;
    public float explosionDamage = 100f;

    public GameObject explosionEffect;
    public float effectDuration = 5f;

    public float effectRadius = 10f;

    public LayerMask impactLayer;

    private void Start()
    {
        StartCoroutine(ExplodeCoroutine());
    }

    public IEnumerator ExplodeCoroutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    public void Explode()
    {
        switch (grenadeType)
        {
            case GrenadeType.Frag:
                ExplodeFrag();
                break;
            case GrenadeType.Smoke:
                ExplodeSmoke();
                break;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == impactLayer)
        {
            ExplodeFrag();
        }
    }
    
    private void ExplodeFrag()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var collider in colliders)
        {
            Health health = collider.GetComponent<Health>();
            if (health != null)
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                float damage = Mathf.Lerp(explosionDamage, 0, distance / explosionRadius);
                health.TakeDamage(damage);
            }
        }

        Destroy(Instantiate(explosionEffect, transform.position, Quaternion.identity),.5f);
        Destroy(gameObject);
    }
    
    private void ExplodeSmoke()
    {
        Destroy(gameObject);
        Destroy(Instantiate(explosionEffect, transform.position, Quaternion.identity),effectDuration);
    }
}
