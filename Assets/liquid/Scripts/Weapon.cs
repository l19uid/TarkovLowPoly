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
    private Vector3 recoilRotation = Vector3.zero;
    
    private Vector3 mouseInputPosition = Vector3.zero;
    private Vector3 mouseInputRotation = Vector3.zero;
    
    private Vector3 keyboardInputPosition = Vector3.zero;
    private Vector3 keyboardInputRotation = Vector3.zero;
    
    private Vector3 idlePosition = Vector3.zero;
    private Vector3 idleRotation = Vector3.zero;
    
    [SerializeField]
    private float spread = 10;
    [SerializeField]
    private float bulletSpeed = 100;
    [SerializeField]
        float bulletWeight = .2f;
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

        MouseInputMovement();
        KeyboardInputMovement();
        //if(Time.time % 1 == 0)
            //IdleWeaponMovement();
        
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

    private void CalculateBullet()
    {
        // Adjust the bullet drop
        Vector3 dropDirection = muzzlePoint.forward + Vector3.down / (gravity / bulletWeight * 40f);
        // bullet hit something
        if (Physics.Raycast(muzzlePoint.position, dropDirection, out hit, range))
        {
            BulletTravel(hit.distance, dropDirection);
        }
    }
    
    private IEnumerable BulletTravel(float distance, Vector3 dropDirection)
    {
        yield return new WaitForSeconds(distance / bulletSpeed);
        // bullet hit something
        if (Physics.Raycast(muzzlePoint.position, dropDirection, out hit, range))
        {
            // bullet hit something
            Debug.Log("Bullet hit " + hit.collider.name);
        }
    }
     
    private void Reload()
    {
        if (currentMagazine <= 0) return;
        isReloading = true;
        StartCoroutine(ReloadTimer());
    }
    
    private void ManageRecoil()
    {
        Vector3 newPosition = recoilPosition + mouseInputPosition + keyboardInputPosition;
        Quaternion newRotation = Quaternion.Euler(
              keyboardInputRotation.x,
            recoilRotation.x + keyboardInputRotation.y - mouseInputRotation.x, 
            mouseInputRotation.y + recoilRotation.y);
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, Time.deltaTime * ergonomics);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, newRotation, Time.deltaTime * ergonomics);
        
        // RESET ALL ROTATIONS AND POSITONS
        recoilPosition = Vector3.Lerp(recoilPosition, originalPosition, Time.deltaTime * ergonomics /2);
        recoilRotation = Vector3.Lerp(recoilRotation, originalRotation.eulerAngles, Time.deltaTime * ergonomics /2);

        keyboardInputPosition = Vector3.Lerp(keyboardInputPosition, originalPosition, Time.deltaTime * ergonomics /2);
        keyboardInputRotation = Vector3.Lerp(keyboardInputRotation, originalRotation.eulerAngles, Time.deltaTime * ergonomics /2);

        mouseInputPosition = Vector3.Lerp(mouseInputPosition, originalRotation.eulerAngles, Time.deltaTime * ergonomics /2);
        mouseInputRotation = Vector3.Lerp(mouseInputRotation, originalPosition, Time.deltaTime * ergonomics /2);
    }
    
    private void CalculateRecoil()
    {
        recoilPosition = transform.localPosition + new Vector3(Random.Range(0, ergonomics) / 100, 0, 0);
        
        recoilRotation = transform.localRotation * new Vector3(
            Random.Range(-horzizontalRecoil / 3, horzizontalRecoil / 3) / 25, 
            Random.Range(-verticalRecoil / 2, -verticalRecoil) / 20, 
            0
            );
    }
    
    private void MouseInputMovement()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        mouseInputRotation = new Vector3(mouseX * ergonomics,mouseY * ergonomics , 0);
        mouseInputPosition = new Vector3(0, -mouseY/ergonomics, -mouseX/ergonomics);
    }
    
    private void KeyboardInputMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        
        keyboardInputPosition = new Vector3(0, 0, horizontal / (10 + ergonomics));
        keyboardInputRotation = new Vector3(0, horizontal / (10 + ergonomics), 0);
    }
    
    private void IdleWeaponMovement()
    {
        idlePosition = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        idleRotation = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
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
        Gizmos.color = Color.red;
        Vector3 dropDirection = muzzlePoint.forward + Vector3.down / (gravity / bulletWeight * 40f);
        Gizmos.DrawLine(muzzlePoint.position, muzzlePoint.position + dropDirection * range);
    }
}
