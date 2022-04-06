using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public float MouseSensitivity = 200f;
    public float MouseMultiplier = 5f;
    public float xRotation = 0f;

    public Transform Player;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity * MouseMultiplier * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity * MouseMultiplier * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        Player.Rotate(Vector3.up * mouseX);
    }
}
