using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private List<Transform> weaponSpawnPoints;

    private readonly List<Weapon> _weapons = new();
    private WeaponsService _weaponsService;
    private WeaponFactory _weaponFactory;

    public event Action OnAllWeaponsDestroyed;

    [Inject]
    public void Construct(WeaponsService weaponsService, WeaponFactory factory)
    {
        _weaponsService = weaponsService;
        _weaponFactory = factory;
    }

    public void InitializeWeapons()
    {
        var selectedWeapons = _weaponsService.GetSelectedWeapons();

        for (int i = 0; i < selectedWeapons.Count; i++)
        {
            var so = selectedWeapons[i];
            Vector3 pos = weaponSpawnPoints[i].position;
            var go = Instantiate(so.WeaponGO, pos, Quaternion.identity, weaponSpawnPoints[i]);

            var weapon = _weaponFactory.AddWeaponComponent(go, so);
            _weapons.Add(weapon);

            weapon.OnWeaponDestroyed += () =>
            {
                _weapons.Remove(weapon);
                CheckAllWeaponsDestroyed();
            };
        }
    }
    public Weapon GetRandomWeapon()
    {
        if (_weapons.Count == 0) return null;
        int randomIndex = Random.Range(0, _weapons.Count);
        return _weapons[randomIndex];
    }

    private void CheckAllWeaponsDestroyed()
    {
        if (_weapons.Count == 0)
        {
            OnAllWeaponsDestroyed?.Invoke();
        }
    }
}
