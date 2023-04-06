using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    // There is a list of magazines and the one used is also at the position 0
    // when you reaload the current magazine is moved to the end of the list
    
    [Header("Magazines")]
    private int bulletCount = 0;
    [SerializeField]
    public Magazine currentMagazine;
    [SerializeField]
    private int maxMagazines = 5;
    
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
    
    private Vector3 newPosition = Vector3.zero;
    private Quaternion newRotation = Quaternion.identity;
    
    public GameObject bulletPrefab;
    [SerializeField]
    private float spread = 10;
    [SerializeField]
    private float bulletSpeed = 100; //MpS
    [SerializeField]
        float bulletWeight = .2f;
    [SerializeField]
    private float bulletPenetration = 10;
    
    private bool _isReloading = false;
    private bool _isFiring = false;
    private bool _isAiming = false;
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
    CameraShake cameraShake;
    
    RaycastHit hit;
    
    List<Magazine> _magazines;
    
    
    private void Start()
    {
        bulletCount = _magazines[0].currentAmmo;
        
        originalRotation = transform.localRotation;
        originalPosition = transform.localPosition;

        FillMags();
    }

    private void FillMags()
    {
        for (int i = 0; i < maxMagazines; i++)
        {
            _magazines.Add((currentMagazine));
        }
        
        foreach (var mag in _magazines)
        {
            mag.SetAmmo(mag.maxAmmo);
        }
    }
    private void Update()
    {
        if (_isReloading) return;

        if (fullAuto && Input.GetButton("Fire1") && !_isFiring && bulletCount > 0)
            StartCoroutine(Shoot());
        else if (!fullAuto && Input.GetButtonDown("Fire1") && !_isFiring && bulletCount > 0)
            StartCoroutine(Shoot());
        
        if (Input.GetKeyDown(KeyCode.R))
            Reload();

        MouseInputMovement();
        KeyboardInputMovement();
        
        ManageRecoil();
    }

    private IEnumerator Shoot()
    {
        _isFiring = true;
        bulletCount--;
        CalculateRecoil();
        CameraShake(1f,  new Quaternion(recoilRotation.x, recoilRotation.y, recoilRotation.z, 1));
        CalculateBullet();
        yield return new WaitForSeconds(1 / fireRate);
        _isFiring = false;
    }

    private void CalculateBullet()
    {
        // Adjust the bullet drop
        Vector3 dropDirection = muzzlePoint.forward + Vector3.down / (gravity / bulletWeight * 20f);
        // bullet hit something
        if (Physics.Raycast(muzzlePoint.position, dropDirection, out hit, range))
        {
            StartCoroutine(BulletTravel(hit.distance, dropDirection));
        }
    }
    
    private IEnumerator BulletTravel(float distance, Vector3 dropDirection)
    {
        yield return new WaitForSeconds(distance / bulletSpeed);
        // bullet hit something
        if (Physics.Raycast(muzzlePoint.position, dropDirection, out hit, range))
        {
            GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, Quaternion.identity);
            Destroy(bullet,distance / bulletSpeed);
            float time = distance / bulletSpeed;
            while(time > 0)
            {
                bullet.transform.position += Vector3.Lerp(bullet.transform.position + dropDirection, hit.point, Time.deltaTime * distance / bulletSpeed);
                time -= Time.deltaTime;
            }
        }
    }
     
    private void Reload()
    {
        _isReloading = true;
        StartCoroutine(ReloadTimer());
    }
    
    private void ManageRecoil()
    {
         newPosition = recoilPosition + mouseInputPosition + keyboardInputPosition;
         newRotation = Quaternion.Euler(
             0, // LEAN LEFT AND RIGHT
             recoilRotation.x - mouseInputRotation.x + keyboardInputRotation.y, // ROTATE LEFT AND RIGHT
             recoilRotation.y + mouseInputRotation.y); // LEAN UP AND DOWN
        
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, Time.deltaTime * ergonomics);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, newRotation, Time.deltaTime * ergonomics);
        
        // RESET ALL ROTATIONS AND POSITONS
        recoilPosition = Vector3.Lerp(recoilPosition, originalPosition, Time.deltaTime * ergonomics / 3);
        recoilRotation = Vector3.Lerp(recoilRotation, originalRotation.eulerAngles, Time.deltaTime * ergonomics / 3);

        keyboardInputPosition = Vector3.Lerp(keyboardInputPosition, originalPosition, Time.deltaTime * ergonomics / 2);
        keyboardInputRotation = Vector3.Lerp(keyboardInputRotation, originalRotation.eulerAngles, Time.deltaTime * ergonomics / 2);

        mouseInputPosition = Vector3.Lerp(mouseInputPosition, originalRotation.eulerAngles, Time.deltaTime * ergonomics / 2);
        mouseInputRotation = Vector3.Lerp(mouseInputRotation, originalPosition, Time.deltaTime * ergonomics / 2);
    }
    
    private void CalculateRecoil()
    {
        recoilPosition = transform.localPosition + new Vector3(Random.Range(ergonomics/4, ergonomics/4) / 100, 0, 0);
        
        recoilRotation += transform.localRotation * new Vector3(
            Random.Range(-horzizontalRecoil / 2, horzizontalRecoil / 2) / 50, // TILT LEFT AND RIGHT
            Random.Range(-verticalRecoil / 2, -verticalRecoil) / 50, // TILT UP
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
        float vertical = Input.GetAxis("Vertical");
        
        keyboardInputPosition = new Vector3(vertical / (ergonomics * 5), 0, horizontal / (ergonomics * 5));
        keyboardInputRotation = new Vector3(0, -horizontal * (ergonomics / 50), 0);
    }
    
    private void CameraShake(float magnitude, Quaternion rotation)
    {
        cameraShake.Shake(1f/fireRate,magnitude,rotation);
    }

    private IEnumerator ReloadTimer()
    {
        yield return new WaitForSeconds(reloadTime);
        
        Magazine mag = _magazines[0];
        _magazines.RemoveAt(0);
        _magazines.Add(mag);
        
        bulletCount = _magazines[0].currentAmmo;
        _isReloading = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(muzzlePoint.position, muzzlePoint.position + muzzlePoint.forward * range);
        Gizmos.color = Color.red;
        Vector3 dropDirection = muzzlePoint.forward + Vector3.down / (gravity / bulletWeight * 20f);
        Gizmos.DrawLine(muzzlePoint.position, muzzlePoint.position + dropDirection * range);
    }
}
