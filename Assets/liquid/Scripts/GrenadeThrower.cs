using System.Collections;
using UnityEngine;

namespace liquid.Scripts
{
    public class GrenadeThrower : MonoBehaviour
    {
        public GameObject grenadePrefab;
        
        public float throwForce = 40f;
        public float throwDelay = 0.5f;
        public float throwCooldown = 0.5f;
        
        private float _throwTimer = 0f;
        
        private void Update()
        {
            _throwTimer += Time.deltaTime;
            
            if (Input.GetKeyDown(KeyCode.G) && _throwTimer >= throwCooldown)
            {
                StartCoroutine(ThrowGrenadeCoroutine());
                _throwTimer = 0f;
            }
        }
        
        public IEnumerator ThrowGrenadeCoroutine()
        {
            yield return new WaitForSeconds(throwDelay);
            ThrowGrenade();
        }
        
        private void ThrowGrenade()
        {
            var grenade = Instantiate(grenadePrefab, transform.position, transform.rotation);
            Vector3 throwDirection = transform.forward + (transform.up * 0.1f);
            grenade.GetComponent<Rigidbody>().AddForce(throwDirection * throwForce, ForceMode.Impulse);
        }
    }
}