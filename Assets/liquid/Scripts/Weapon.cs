using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    [Header("Magazines")]
    private int bulletCount = 0;
    [SerializeField]
    private int magazineSize = 30;
    [SerializeField]
    private int maxMagazines = 5;
    private int currentMagazine = 0;
    
    [Header("Stats")]
    [SerializeField]
    private float damage = 10;
    [SerializeField]
    private float fireRate = 10;
    [SerializeField]
    private float reloadTime = 2;
    
    [Header("Recoil")]
    [SerializeField]
    private float horzizontalRecoil = 10;
    [SerializeField]
    private float verticalRecoil = 10;
    [SerializeField]
    private float ergonomics = 10;
    
    private Vector3 originalPosition = Vector3.zero;
    private Quaternion originalRotation = Quaternion.identity;
    
    private Vector3 recoilPosition = Vector3.zero;
    private Quaternion recoilRotation = Quaternion.identity;
    
    [SerializeField]
    private float spread = 10;
    [SerializeField]
    private float bulletSpeed = 100;
    [SerializeField]
    private float bulletDrop = 10;
    [SerializeField]
    private float bulletPenetration = 10;
    
    private bool isReloading = false;
    private bool isFiring = false;
    private bool isAiming = false;
    [SerializeField]
    private bool fullAuto = false;
    
    public Transform muzzlePoint;
    [SerializeField]
    float muzzleVelocity = 800f; // meters per second
    [SerializeField]
    float range = 1000f; // meters
    [SerializeField]
    float gravity = 9.81f;
    [SerializeField]
    float bulletWeight = .34f;
    
    RaycastHit hit;
    
    private void Start()
    {
        currentMagazine = maxMagazines;
        bulletCount = magazineSize;
        
        originalRotation = transform.localRotation;
        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (isReloading) return;

        if (fullAuto && Input.GetButton("Fire1") && !isFiring && bulletCount > 0)
            StartCoroutine(Shoot());
        else if (!fullAuto && Input.GetButtonDown("Fire1") && !isFiring && bulletCount > 0)
            StartCoroutine(Shoot());
        
        if (Input.GetKeyDown(KeyCode.R))
            Reload();

        ManageRecoil();
    }

    private IEnumerator Shoot()
    {
        isFiring = true;
        bulletCount--;
        CalculateRecoil();
        CalculateBullet();
        yield return new WaitForSeconds(1 / fireRate);
        isFiring = false;
    }
    
    private void CalculateRecoil()
    {
        recoilPosition = transform.localPosition + new Vector3(Random.Range(0, ergonomics/25) / 25, 0, 0);
        
        recoilRotation = transform.localRotation * Quaternion.Euler(
            0, 
            Random.Range(-horzizontalRecoil, horzizontalRecoil) / 20, 
            Random.Range(0, -verticalRecoil) / 20);
    }
    
    private void CalculateBullet()
    {
        // adjust the shooter's aim based on the drop
        Physics.Raycast(muzzlePoint.position, muzzlePoint.forward, out hit, range);
        if(hit.distance > 0)
            bulletDrop = (hit.distance / range) * (gravity*bulletWeight);
        else
            range = (hit.distance / range) * (gravity*bulletWeight);
        
        Vector3 dropDirection = muzzlePoint.forward + Vector3.down / (gravity / bulletWeight);
        // bullet hit something
        if (Physics.Raycast(muzzlePoint.position, dropDirection, out hit, range))
        {
            // bullet hit something
            Debug.Log("Bullet hit " + hit.collider.name);
        }
    }
    
    private void ManageRecoil()
    {
        // Moves the weapon to the recoil position and rotation
        transform.localPosition = Vector3.Lerp(transform.localPosition, recoilPosition, Time.deltaTime * ergonomics);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, recoilRotation, Time.deltaTime * ergonomics);
        
        // Moves the weapon back to the original position and rotation
        recoilRotation = Quaternion.Lerp(recoilRotation, originalRotation, Time.deltaTime * ergonomics /2);
        recoilPosition = Vector3.Lerp(recoilPosition, originalPosition, Time.deltaTime * ergonomics /2);
    }
    
    private void Reload()
    {
        if (currentMagazine <= 0) return;
        isReloading = true;
        StartCoroutine(ReloadTimer());
    }
    
    private IEnumerator ReloadTimer()
    {
        yield return new WaitForSeconds(reloadTime);
        currentMagazine--;
        bulletCount = magazineSize;
        isReloading = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(muzzlePoint.position, muzzlePoint.position + muzzlePoint.forward * range);
        Vector3 dropDirection = muzzlePoint.forward + Vector3.down / (gravity / bulletWeight);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(muzzlePoint.position, muzzlePoint.position + dropDirection * range);
    }
}
