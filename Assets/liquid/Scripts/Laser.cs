using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    public Color laserColor;
    private LineRenderer laserLine;
    public float laserWidth;
    public float laserMaxLength;
    public float laserFadeSpeed;
    
    void Start()
    {
        laserLine = GetComponent<LineRenderer>();
        laserLine.material.color = laserColor;
        laserLine.startWidth = laserWidth;
        laserMaxLength = 100f;
    }

    void Update()
    {
        laserLine.SetPosition(0, transform.position);
        //Calculate the position of the hit point of the laser
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, laserMaxLength))
        {
            laserLine.SetPosition(1, hit.point);
        }
        else
        {
            laserLine.SetPosition(1, transform.position + transform.forward * laserMaxLength);
        }
    }
}
