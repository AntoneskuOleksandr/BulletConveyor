using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Transform weaponSpawnRoot;
    [SerializeField] private float spawnOffset = 2f;

    private readonly List<Weapon> _weapons = new();
    private WeaponsService _weaponsService;
    private WeaponFactory _weaponFactory;

    [Inject]
    public void Construct(WeaponsService weaponsService, WeaponFactory factory)
    {
        _weaponsService = weaponsService;
        _weaponFactory = factory;
    }

    public void InitializeWeapons()
    {
        var selectedWeapons = _weaponsService.GetSelectedWeapons();
        float angleStep = 360f / selectedWeapons.Count;

        for (int i = 0; i < selectedWeapons.Count; i++)
        {
            var so = selectedWeapons[i];
            Vector3 pos = weaponSpawnRoot.position + Quaternion.Euler(0, angleStep * i, 0) * Vector3.forward * spawnOffset;
            var go = Instantiate(so.WeaponGO, pos, Quaternion.identity, weaponSpawnRoot);

            var weapon = _weaponFactory.AddWeaponComponent(go, so);
            _weapons.Add(weapon);
        }
    }
    public Weapon GetRandomWeapon()
    {
        if (_weapons.Count == 0) return null;
        int randomIndex = Random.Range(0, _weapons.Count);
        return _weapons[randomIndex];
    }
}
