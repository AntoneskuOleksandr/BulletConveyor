using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

[Serializable]
public class WeaponEffectChance
{
    public OnHitEffectType effectType;
    [Range(0f, 1f)] public float chance;
}

[RequireComponent(typeof(WeaponAttachmentPoints), typeof(WeaponVisual))]
public class Weapon : MonoBehaviour
{
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
    public Action OnWeaponDestroyed;

    private List<WeaponEffectChance> baseWeaponEffectChances = new();

    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    private WeaponVisual _weaponVisual;
    private WeaponStatusCanvas _weaponStatusCanvas;
    private WeaponHitZone _weaponHitZone;

    private Enemy _target;
    public Enemy Target => _target;

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
    [SerializeField] private BulletMoveType bulletMoveType;
    [SerializeField] private BulletHitType bulletHitType;

    public IBulletMoveStrategy BulletMoveStrategy => _bulletMoveStrategy;
    public IBulletHitStrategy BulletHitStrategy => _bulletHitStrategy;


    private IBulletMoveStrategy _bulletMoveStrategy;
    private IBulletHitStrategy _bulletHitStrategy;

    private IShootStrategy _shootStrategy;
    private IAimStrategy _aimStrategy;
    private ITargetStrategy _targetStrategy;

    /// <summary>
    /// Назначение стратегий поведения
    /// </summary>
    public void SetWeaponStrategy(IShootStrategy shoot, ShootStrategyType shootType,
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

    public void SetBulletStrategy(IBulletMoveStrategy move, BulletMoveType moveType,
        IBulletHitStrategy hit, BulletHitType hitType)
    {
        _bulletMoveStrategy = move;
        _bulletHitStrategy = hit;
        bulletMoveType = moveType;
        bulletHitType = hitType;
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

        baseWeaponEffectChances.AddRange(so.baseEffectChances);

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

        foreach (var effectChance in baseWeaponEffectChances)
        {
            _effectChanceManager.RegisterWeaponChance(
                this,
                effectChance.effectType,
                effectChance.chance
            );
        }

        _weaponVisual = GetComponent<WeaponVisual>();
        _weaponStatusCanvas = GetComponentInChildren<WeaponStatusCanvas>();
        _weaponHitZone = GetComponent<WeaponHitZone>();

        if (_weaponHitZone != null)
            _weaponHitZone.Initialize(this);

        if (_weaponVisual == null)
            Debug.LogError($"[Weapon] На объекте {gameObject.name} нет WeaponVisual");
        if (_weaponStatusCanvas == null)
            Debug.LogError($"[Weapon] На объекте {gameObject.name} нет WeaponStatusCanvas");
    }

    protected virtual void OnDisable()
    {
        if (_target != null)
            _target.OnDeath.RemoveListener(OnTargetDeath);
    }

    protected void Update()
    {
        _targetRefreshTimer -= Time.deltaTime;
        if (_targetRefreshTimer <= 0f)
        {
            _targetRefreshTimer = _targetRefreshRate;
            _target = _targetStrategy?.FindTarget(this);
        }

        if (_target != null && _target.IsAlive)
        {
            _idleTimer = 0f;

            if (_aimStrategy?.TryAim(this) == true)
            {
                if (_shootStrategy?.TryShoot(this) == true)
                {
                    var effectChances = _effectChanceManager.GetAllChancesForWeapon(this);

                    var colorWeights = _effectFactory.ConvertChancesToColors(effectChances);

                    _weaponVisual?.PlayShotEffect(colorWeights);
                }
            }

        }
        else
        {
            HandleIdleRotation();
        }

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
        _target = null;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        health -= amount;

        _weaponStatusCanvas?.UpdateHealthBar(health, Data.baseHealth);

        if (health <= 0f)
        {
            OnDestroyed();
        }
    }

    private void OnDestroyed()
    {
        _target = null;
        _shootStrategy = null;
        _aimStrategy = null;
        _targetStrategy = null;

        var colls = GetComponents<Collider>();
        foreach (var c in colls) c.enabled = false;

        //if (_weaponVisual != null) _weaponVisual.PlayDestroyEffect();

        OnWeaponDestroyed?.Invoke();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
