using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    //Rotations
    private Vector3 currentRotation;
    private Vector3 targetRotation;

    [Header("Settings")]
    [SerializeField] private float snapiness;
    [SerializeField] private float returnSpeed;

    void Update()
    {
        targetRotation = Vector3.Lerp(targetRotation, Vector3.zero, returnSpeed * Time.deltaTime);
        currentRotation = Vector3.Slerp(currentRotation, targetRotation, snapiness * Time.fixedDeltaTime);

        transform.localRotation = Quaternion.Euler(currentRotation);
    }

    public void RecoilFire(float recoilX, float recoilY, float recoilZ)
    {
        targetRotation += new Vector3(recoilX, Random.Range(-recoilY, recoilY), recoilZ);
    }
}
