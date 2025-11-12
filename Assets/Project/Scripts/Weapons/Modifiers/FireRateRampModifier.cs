using System.Collections.Generic;
using UnityEngine;

public class FireRateRampModifier : IWeaponModifier, IStatProvider
{
    private float _fireRateIncreasePerShot;
    private float _maxFireRateBonus;
    private float _resetTime;

    private Enemy _lastTarget;
    private float _currentBonus;
    private float _lastShotTime;

    public void Initialize(Weapon weapon)
    {
        _fireRateIncreasePerShot = weapon.Data.GetParameter<float>(WeaponParameterType.FireRateIncreasePerShot);
        _maxFireRateBonus = weapon.Data.GetParameter<float>(WeaponParameterType.MaxFireRateBonus);
        _resetTime = weapon.Data.GetParameter<float>(WeaponParameterType.FireRateRampResetTime);

        _currentBonus = 0f;
        _lastTarget = null;
        _lastShotTime = Mathf.NegativeInfinity;
    }

    public void OnBeforeShoot(Weapon weapon)
    {
        float timeSinceLastShot = Time.time - _lastShotTime;
        Enemy currentTarget = weapon.Target;

        if (currentTarget == null || currentTarget != _lastTarget || timeSinceLastShot > _resetTime)
        {
            _currentBonus = 0f;
        }

        _lastTarget = currentTarget;
    }

    public void OnAfterShoot(Weapon weapon, List<ProjectileBase> projectiles)
    {
        _currentBonus = Mathf.Min(_currentBonus + _fireRateIncreasePerShot, _maxFireRateBonus);
        _lastShotTime = Time.time;
    }

    public float ModifyFireRate(float baseValue)
    {
        return baseValue * (1f + _currentBonus);
    }
}
