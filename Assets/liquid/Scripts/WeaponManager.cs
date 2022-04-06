using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    public float pickupRange;
    public float pickupRadius;

    public int weaponLayer;
    public float swaySize;
    public float swaySmooth;

    public float defaultFov;
    public float scopedFov;
    public float fovSmooth;

    public Transform weaponHolder;
    public Transform playerCamera;
    public Transform swayHolder;
    //public TMP_Text ammoText;
    public Camera[] playerCams;
    public Image crosshairImage;
    public Text pickupText;

    private bool _isWeaponHeld;
    private Weapon _heldWeapon;

    private void Update()
    {
        PickUpTextShower();

        PickUp();

        crosshairImage.gameObject.SetActive(!_isWeaponHeld || !_heldWeapon.Scoping);
        foreach (var cam in playerCams)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, _isWeaponHeld && _heldWeapon.Scoping ? scopedFov : defaultFov, fovSmooth * Time.deltaTime);
        }
    }

    private float GetDistanceTo(RaycastHit hit)
    {
        return Vector3.Distance(playerCamera.position, hit.point == Vector3.zero ? hit.transform.position : hit.point);
    }

    public void PickUpTextShower()
    {
        var textHitList = new RaycastHit[256];
        var textHitNumber = Physics.CapsuleCastNonAlloc(playerCamera.position,
            playerCamera.position + playerCamera.forward * pickupRange, pickupRadius, playerCamera.forward,
            textHitList);

        var textRealList = new List<RaycastHit>();
        for (var i = 0; i < textHitNumber; i++)
        {
            var hit = textHitList[i];
            if (hit.transform.gameObject.layer != weaponLayer)
            {
                pickupText.text = "";
                continue;
            }
            if (hit.point == Vector3.zero)
            {
                pickupText.text = "Press -E- to pickup.";
            }
            else if (Physics.Raycast(playerCamera.position, hit.point - playerCamera.position, out var hitInfo,
                hit.distance + 0.1f) && hitInfo.transform == hit.transform)
            {
                pickupText.text = "Press -E- to pickup.";
            }
        }

    }
    
    public void PickUp()
    {
        if (_isWeaponHeld)
        {
            var mouseDelta = -new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            swayHolder.localPosition = Vector3.Lerp(swayHolder.localPosition, Vector3.zero, swaySmooth * Time.deltaTime);
            // swayHolder.localPosition += (Vector3)mouseDelta * swaySize;

            if (Input.GetKeyDown(KeyCode.Q))
            {
                _heldWeapon.Drop(playerCamera);
                _heldWeapon = null;
                _isWeaponHeld = false;
            }
        }
        else if (Input.GetKeyDown("e"))
        {
            var hitList = new RaycastHit[256];
            var hitNumber = Physics.CapsuleCastNonAlloc(playerCamera.position,
                playerCamera.position + playerCamera.forward * pickupRange, pickupRadius, playerCamera.forward,
                hitList);

            var realList = new List<RaycastHit>();
            for (var i = 0; i < hitNumber; i++)
            {
                var hit = hitList[i];
                if (hit.transform.gameObject.layer != weaponLayer) continue;
                if (hit.point == Vector3.zero)
                {
                    realList.Add(hit);
                }
                else if (Physics.Raycast(playerCamera.position, hit.point - playerCamera.position, out var hitInfo,
                    hit.distance + 0.1f) && hitInfo.transform == hit.transform)
                {
                    realList.Add(hit);
                }
            }

            if (realList.Count == 0)
            {
                return;
            }

            realList.Sort((hit1, hit2) => {
                var dist1 = GetDistanceTo(hit1);
                var dist2 = GetDistanceTo(hit2);

                return Mathf.Abs(dist1 - dist2) < 0.001f ? 0 : dist1 < dist2 ? -1 : 1;
            });

            _isWeaponHeld = true;
            _heldWeapon = realList[0].transform.GetComponent<Weapon>();
            _heldWeapon.Pickup(weaponHolder, playerCamera);
        }
    }
}
