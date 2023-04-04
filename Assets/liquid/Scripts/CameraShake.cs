using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    Quaternion recoilRotation = Quaternion.identity;
    
    private void Update()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, recoilRotation, Time.deltaTime * 5f);
    }

    public void Shake(float duration, float magnitude, Quaternion rotation)
    {
        recoilRotation = Quaternion.Euler(rotation.y * magnitude, rotation.x * magnitude, rotation.z * magnitude);
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        yield return new WaitForSeconds(duration);
        
        recoilRotation = Quaternion.identity;
    }
}
