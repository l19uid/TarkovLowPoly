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
    
    [Header("Muzzle Flash")]
    public GameObject muzzleFlashEffect;

    [Header("Recoil")]
    [SerializeField]
    private Vector3 defaultPosition;
    [SerializeField]
    private Vector3 scopePosition;
    [SerializeField]
    private bool isScoped = false;    
    [SerializeField]
    private float horzizontalRecoil = 10;
    [SerializeField]
    private float verticalRecoil = 10;
    [SerializeField]
    private float ergonomics = 10;
    
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

    private GameObject camera;
    private Camera mainCamera;
    private Camera weaponCamera;
    
    
    private void Start()
    {
        FillMags();

        bulletCount = _magazines[0].currentAmmo;
        
        originalRotation = new Quaternion(0,0,0,0);
        
        camera = GameObject.Find("Camera");
        mainCamera =  GameObject.Find("Main Camera").GetComponent<Camera>();
        weaponCamera = GameObject.Find("Weapon Camera").GetComponent<Camera>();
    }

    private void FillMags()
    {
        _magazines = new List<Magazine>();
        currentMagazine.currentAmmo = currentMagazine.maxAmmo;
        
        for (int i = 0; i < maxMagazines; i++)
        {
            _magazines.Add(currentMagazine);
        }
    }
    private void Update()
    {
        if (_isReloading) return;

        if (Input.GetButtonDown("Fire2"))
            Scope();
        ManageFOW();

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

    private void Scope()
    {
        isScoped = !isScoped;
    }
    
    private void ManageFOW()
    {
        if (isScoped)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, 60, Time.deltaTime * 25);
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, 40, Time.deltaTime * 15);
        }
        else
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, 90, Time.deltaTime * 25);
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCamera.fieldOfView, 70, Time.deltaTime * 25);
        }
    }

    private IEnumerator Shoot()
    {
        _isFiring = true;
        bulletCount--;
        
        if(Random.Range(0, 6) < 4)
            Destroy(Instantiate(muzzleFlashEffect, muzzlePoint.position, camera.transform.rotation), 1f);
        
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
            recoilRotation.y + mouseInputRotation.y, // LEAN UP AND DOWN
            recoilRotation.x - mouseInputRotation.x + keyboardInputRotation.y, // ROTATE LEFT AND RIGHT
            0); // LEAN UP AND DOWN
        
         
        Vector3 resetPosition = defaultPosition; 
        Quaternion resetRotation = originalRotation;

        if (isScoped)
        {
            resetPosition = scopePosition;
            newPosition.z *= 0.5f;
            newPosition.x *= 0.85f;
            newRotation *= Quaternion.Euler(0.5f, 0.5f, 0.5f);
        }
         
        transform.localPosition = Vector3.Lerp(transform.localPosition, newPosition, Time.deltaTime * ergonomics + 10);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, newRotation, Time.deltaTime * ergonomics + 10);
        
        // RESET ALL ROTATIONS AND POSITONS
        recoilPosition = Vector3.Lerp(recoilPosition, resetPosition, Time.deltaTime * ergonomics / 3);
        recoilRotation = Vector3.Lerp(recoilRotation, resetRotation.eulerAngles, Time.deltaTime * ergonomics / 3);

        keyboardInputPosition = Vector3.Lerp(keyboardInputPosition, resetPosition, Time.deltaTime * ergonomics / 2);
        keyboardInputRotation = Vector3.Lerp(keyboardInputRotation, resetRotation.eulerAngles, Time.deltaTime * ergonomics / 2);

        mouseInputPosition = Vector3.Lerp(mouseInputPosition, resetPosition, Time.deltaTime * ergonomics / 2);
        mouseInputRotation = Vector3.Lerp(mouseInputRotation, resetRotation.eulerAngles, Time.deltaTime * ergonomics / 2);
    }
    
    private void CalculateRecoil()
    {
        recoilPosition = transform.localPosition + new Vector3(0, Random.Range(verticalRecoil/100, verticalRecoil/100) / 1000, 0);
        
        recoilRotation += transform.localRotation * new Vector3(
            Random.Range(-horzizontalRecoil / 2, horzizontalRecoil / 2) / 40, // TILT LEFT AND RIGHT
            Random.Range(-verticalRecoil / 2, -verticalRecoil) / 40, // TILT UP
            0
            );
    }
    
    private void MouseInputMovement()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        mouseInputRotation = new Vector3(mouseX * ergonomics / 5,mouseY * ergonomics / 5 , 0);
        mouseInputPosition = new Vector3(-mouseX/ergonomics, -mouseY/ergonomics, 0);
    }
    
    private void KeyboardInputMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        keyboardInputPosition = new Vector3(horizontal / 100, 0, -vertical / 100);
        keyboardInputRotation = new Vector3(0, -horizontal / 10, 0);
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
        
        Gizmos.DrawCube(muzzlePoint.position, .1f * Vector3.one);
    }
}
