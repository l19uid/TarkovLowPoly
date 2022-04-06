using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Weapon : MonoBehaviour
{
    [Header("Throwing")]
    public float throwForce;
    public float throwExtraForce;
    public float rotationForce;

    [Header("Pickup")]
    public float animTime;

    [Header("Shooting")]
    public int maxAmmo;
    public int shotsPerSecond;
    public float reloadSpeed;
    public float hitForce;
    public float range;
    public bool tapable;
    public float kickbackForce;
    public float resetSmooth;
    public Vector3 scopePos;

    [Header("Recoil")]
    [SerializeField] private float recoilX;
    [SerializeField] private float recoilY;
    [SerializeField] private float recoilZ;
    public GameObject bolt;

    [Header("Data")]
    public int weaponGfxLayer;
    public GameObject[] weaponGfxs;
    public Collider[] gfxColliders;

    private float _rotationTime;
    private float _time;
    private bool _held;
    private bool _scoping;
    private bool _reloading;
    private bool _shooting;
    private int _ammo;
    private Rigidbody _rb;
    private LineRenderer _shootingLine;
    private Transform _playerCamera;
    private TMP_Text _ammoText;
    private Vector3 _startPosition;
    private Quaternion _startRotation;
    [Header("Game Objects")]
    public TMP_Text ammoCount;
    public Camera fpsCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;


    private float nextTimeToFire = 0f;
    private CameraRecoil recoilScript;
    public GameObject recoil;

    private void Start()
    {
        _rb = gameObject.AddComponent<Rigidbody>();
        _shootingLine = impactEffect.GetComponent<LineRenderer>();
        _rb.mass = 0.1f;
        _ammo = maxAmmo;
        recoilScript = recoil.GetComponent<CameraRecoil>();
    }

    private void Update()
    {
        if (!_held) return;

        if (_time < animTime)
        {
            _time += Time.deltaTime;
            _time = Mathf.Clamp(_time, 0f, animTime);
            var delta = -(Mathf.Cos(Mathf.PI * (_time / animTime)) - 1f) / 2f;
            transform.localPosition = Vector3.Lerp(_startPosition, Vector3.zero, delta);
            transform.localRotation = Quaternion.Lerp(_startRotation, Quaternion.identity, delta);
        }
        else
        {
            _scoping = Input.GetMouseButton(1) && !_reloading;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = Vector3.Lerp(transform.localPosition, _scoping ? scopePos : Vector3.zero, resetSmooth * Time.deltaTime);
        }

        if (_reloading)
        {
            _rotationTime += Time.deltaTime;
            var spinDelta = -(Mathf.Cos(Mathf.PI * (_rotationTime / reloadSpeed)) - 1f) / 2f;
            transform.localRotation = Quaternion.Euler(new Vector3(spinDelta * 360f, 0, 0));
        }

        if ((tapable ? Input.GetMouseButtonDown(0) : Input.GetMouseButton(0)) && !_shooting && !_reloading)
        {
            Shoot();
            ammoCount.text = _ammo + " / " + maxAmmo;

            bolt.transform.localPosition = bolt.transform.localPosition - new Vector3(0, -.1f, 0);
            StartCoroutine(ShootingCooldown());
            bolt.transform.localPosition = Vector3.zero;
        }
    }

    private void Shoot()
    {
        muzzleFlash.Play();
        _ammo--;

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
            _shootingLine.SetPosition(0, hit.point);
            _shootingLine.SetPosition(1, gameObject.transform.position);

            Destroy(impactGO, .25f);
        }

        recoilScript.RecoilFire(recoilX, recoilX, recoilZ);
        //StartCoroutine(ShootingCooldown());
    }

    private IEnumerator ShootingCooldown()
    {
        _shooting = true;
        yield return new WaitForSeconds(1f / shotsPerSecond);
        _shooting = false;
    }

    public void Pickup(Transform weaponHolder, Transform playerCamera)
    {
        if (_held) return;
        Destroy(_rb);
        _time = 0f;
        transform.parent = weaponHolder;
        _startPosition = transform.localPosition;
        _startRotation = transform.localRotation;
        foreach (var col in gfxColliders)
        {
            col.enabled = false;
        }
        foreach (var gfx in weaponGfxs)
        {
            gfx.layer = weaponGfxLayer;
        }
        _held = true;
        ammoCount.text = _ammo + " / " + maxAmmo;
        _playerCamera = playerCamera;
        _scoping = false;
    }

    public void Drop(Transform playerCamera)
    {
        if (!_held) return;
        _rb = gameObject.AddComponent<Rigidbody>();
        _rb.mass = 0.1f;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        var forward = playerCamera.forward;
        forward.y = 0f;
        _rb.velocity = forward * throwForce;
        _rb.velocity += Vector3.up * throwExtraForce;
        _rb.angularVelocity = Random.onUnitSphere * rotationForce;
        foreach (var col in gfxColliders)
        {
            col.enabled = true;
        }
        foreach (var gfx in weaponGfxs)
        {
            gfx.layer = 0;
        }
        transform.parent = null;
        _held = false;
    }

    public bool Scoping => _scoping;
}
