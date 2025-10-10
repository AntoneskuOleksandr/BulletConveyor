using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public enum BulletType
{
    Standard,
    Missile
}

[Serializable]
public class WeaponEffectChance
{
    public OnHitEffectType effectType;
    [Range(0f, 1f)] public float chance;
}

public class Weapon : MonoBehaviour
{
    [SerializeField] private BulletType bulletType;
    public BulletType BulletType => bulletType;

    [Header("Weapon Settings")]
    [SerializeField] private float turnSpeed = 3f;
    [SerializeField] private float aimThreshold = 2f;
    [SerializeField] private float idleReturnDelay = 2f;

    private float damage;
    private float health;
    private float fireRate;
    private float attackRadius;

    private Transform firePoint;
    private Transform partToRotate;

    public float Damage => damage;
    public float Health => health;
    public float FireRate => fireRate;
    public float AttackRadius => attackRadius;
    public Transform FirePoint => firePoint;
    public Transform PartToRotate => partToRotate;
    public float TurnSpeed => turnSpeed;
    public float AimThreshold => aimThreshold;

    public WeaponSO Data { get; private set; }

    [Header("Effect Chances from Weapon")]
    [Tooltip("Задайте стартовые шансы на разные дебаффы от этой пушки.")]
    public List<WeaponEffectChance> effectChances = new();

    private EffectChanceManager _effectChanceManager;

    private Enemy target;
    public Enemy Target => target;

    private float _fireCountdown;
    public float FireCountdown => _fireCountdown;

    private float _targetRefreshRate = 1f;
    private float _targetRefreshTimer = 0f;

    private Quaternion _initialRotation;
    private float _idleTimer = 0f;

    [Header("Strategy Info (Read Only)")]
    [SerializeField] private ShootStrategyType shootStrategyType;
    [SerializeField] private AimStrategyType aimStrategyType;
    [SerializeField] private TargetStrategyType targetStrategyType;

    private IShootStrategy _shootStrategy;
    private IAimStrategy _aimStrategy;
    private ITargetStrategy _targetStrategy;

    [Inject]
    public void Construct(EffectChanceManager effectChanceManager)
    {
        _effectChanceManager = effectChanceManager;
    }

    /// <summary>
    /// Назначение стратегий поведения
    /// </summary>
    public void SetStrategies(IShootStrategy shoot, ShootStrategyType shootType,
        IAimStrategy aim, AimStrategyType aimType,
        ITargetStrategy target, TargetStrategyType targetType)
    {
        _shootStrategy = shoot;
        _aimStrategy = aim;
        _targetStrategy = target;

        shootStrategyType = shootType;
        aimStrategyType = aimType;
        targetStrategyType = targetType;
    }

    /// <summary>
    /// Инициализация параметров пушки из ScriptableObject
    /// </summary>
    public virtual void InitializeFromSO(WeaponSO so, WeaponProgressRO progress)
    {
        Data = so;
        damage = so.baseDamage;
        fireRate = so.baseFireRate;
        health = so.baseHealth;
        attackRadius = so.baseAttackRadius;

        // --- Применение апгрейдов ---
        for (int i = 0; i < progress.DamageLevel; i++)
            damage += so.upgradeLevels[i].damageBonus;
        for (int i = 0; i < progress.FireRateLevel; i++)
            fireRate += so.upgradeLevels[i].fireRateBonus;
        for (int i = 0; i < progress.HealthLevel; i++)
            health += so.upgradeLevels[i].healthBonus;

        var attachment = GetComponent<WeaponAttachmentPoints>();
        if (attachment != null)
        {
            firePoint = attachment.firePoint;
            partToRotate = attachment.partToRotate;
        }
        else
        {
            Debug.LogWarning($"[Weapon] На объекте {gameObject.name} нет WeaponAttachmentPoints");
        }
    }

    protected virtual void Start()
    {
        if (partToRotate != null)
            _initialRotation = partToRotate.rotation;

        foreach (var effectChance in effectChances)
        {
            _effectChanceManager.RegisterWeaponChance(
                this,
                effectChance.effectType,
                effectChance.chance
            );
        }
    }

    protected virtual void OnDisable()
    {
        if (target != null)
            target.OnDeath.RemoveListener(OnTargetDeath);
    }

    protected void Update()
    {
        // --- обновляем цель ---
        _targetRefreshTimer -= Time.deltaTime;
        if (_targetRefreshTimer <= 0f)
        {
            _targetRefreshTimer = _targetRefreshRate;
            target = _targetStrategy?.FindTarget(this);
        }

        // --- логика поведения ---
        if (target != null)
        {
            _idleTimer = 0f;
            _aimStrategy?.Aim(this);
            _shootStrategy?.Shoot(this);
        }
        else
        {
            HandleIdleRotation();
        }

        // --- таймер перезарядки ---
        if (_fireCountdown > 0f)
            _fireCountdown -= Time.deltaTime;
    }

    private void HandleIdleRotation()
    {
        _idleTimer += Time.deltaTime;
        if (_idleTimer >= idleReturnDelay && partToRotate != null)
        {
            partToRotate.rotation = Quaternion.Lerp(
                partToRotate.rotation,
                _initialRotation,
                Time.deltaTime * turnSpeed
            );
        }
    }

    public void ResetFireCountdown()
    {
        _fireCountdown = 1f / fireRate;
    }

    public void OnTargetDeath()
    {
        target = null;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
