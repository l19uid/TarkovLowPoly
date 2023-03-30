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
    
    [SerializeField]
    private Vector3 originalPosition = Vector3.zero;
    [SerializeField]
    private Vector3 recoilPosition = Vector3.zero;
    [SerializeField]
    private Quaternion originalRotation = Quaternion.identity;
    [SerializeField]
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
    
    private void Start()
    {
        currentMagazine = maxMagazines;
        bulletCount = magazineSize;
    }

    private void Update()
    {
        if (isReloading) return;
        
        if (Input.GetButtonDown("Fire1") && !isFiring && bulletCount > 0)
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
        recoilPosition = originalPosition + new Vector3(
            Random.Range(-ergonomics, ergonomics) / 200, 
            Random.Range(-ergonomics, ergonomics) / 100, 
            Random.Range(-ergonomics, ergonomics) / 200);;
        recoilRotation = originalRotation * Quaternion.Euler(Random.Range(-horzizontalRecoil, horzizontalRecoil), 
            Random.Range(0, verticalRecoil), 0);
    }
    
    private void CalculateBullet()
    {
        
    }
    
    private void ManageRecoil()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, recoilPosition, Time.deltaTime * 10);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, recoilRotation, Time.deltaTime * 10);
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
}
