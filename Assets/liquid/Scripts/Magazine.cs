using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magazine : MonoBehaviour
{
    public int maxAmmo = 30;
    public int currentAmmo = 0;
    
    public string ammoType = "9mm";
    public string magazineName = "9mm Magazine";
    
    public GameObject magazineModel;
    
    
    public void Reload()
    {
        currentAmmo = maxAmmo;
    }
    
    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
    }
    
    public void RemoveAmmo(int amount)
    {
        currentAmmo -= amount;
    }
    
    public bool IsFull()
    {
        return currentAmmo == maxAmmo;
    }
    
    public bool IsEmpty()
    {
        return currentAmmo == 0;
    }
    
    public void SetAmmo(int amount)
    {
        currentAmmo = amount;
    }
}
