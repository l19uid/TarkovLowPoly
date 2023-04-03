using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CameraShake : MonoBehaviour
{
    Quaternion recoilRotation = Quaternion.identity;
    
    private void Update()
    {
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
    }

    public void Shake(float duration, float magnitude)
    {
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }
    
    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        recoilRotation = new Quaternion(0,Random.Range(-magnitude,magnitude),Random.Range(-magnitude,magnitude),0);

        yield return new WaitForSeconds(duration);
        
        Quaternion originalRotation = transform.localRotation;
    }
}
