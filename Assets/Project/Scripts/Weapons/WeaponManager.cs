using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<Weapon> _weapons = new List<Weapon>();

    public void RegisterWeapon(List<Weapon> weapons)
    {
        _weapons.AddRange(weapons);
    }

    public Weapon GetRandomWeapon()
    {
        if (_weapons.Count == 0)
            return null;
        int randomIndex = Random.Range(0, _weapons.Count);
        return _weapons[randomIndex];
    }
}
