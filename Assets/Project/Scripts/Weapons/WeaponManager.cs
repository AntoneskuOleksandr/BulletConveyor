using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public List<Weapon> weapons; // List of all available weapons
    public float fireRate = 1f; // Time between shots in seconds

    public void Initialize()
    {
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var weapon in weapons)
            {
                weapon.Shoot();
            }
        }
    }
}
