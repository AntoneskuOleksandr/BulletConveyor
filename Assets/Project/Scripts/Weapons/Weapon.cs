using System;
using System.Collections.Generic;
using System.Linq;
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
    private float damage;
    private float health;
    private float fireRate;
    private float attackRadius;
    private float aimThreshold;
    private float idleReturnDelay = 2f;
    private float yawTurnSpeed;
    private float pitchTurnSpeed;

    public float Damage => damage;
    public float Health => health;
    public float FireRate => fireRate;
    public float AttackRadius => attackRadius;
    public float LaunchSpeed => Mathf.Sqrt(attackRadius * Mathf.Abs(Physics.gravity.y));
    public float AimThreshold => aimThreshold;
    public WeaponAttachmentPoints AttachmentPoints => _attachmentPoints;

    private WeaponAttachmentPoints _attachmentPoints;

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

    private float _idleTimer = 0f;

    [Header("Strategy Info (Read Only at inspector)")]
    [SerializeField] private ShootStrategyType shootStrategyType;
    [SerializeField] private List<AimStrategyType> aimStrategyTypes;
    [SerializeField] private TargetStrategyType targetStrategyType;
    [SerializeField] private BulletMoveType bulletMoveType;
    [SerializeField] private BulletHitType bulletHitType;

    public IBulletMoveStrategy BulletMoveStrategy => _bulletMoveStrategy;
    public IBulletHitStrategy BulletHitStrategy => _bulletHitStrategy;

    private IBulletMoveStrategy _bulletMoveStrategy;
    private IBulletHitStrategy _bulletHitStrategy;

    private IShootStrategy _shootStrategy;
    private List<IAimStrategy> _aimStrategies = new();
    private IdleAimStrategy _idleAimStrategy = new IdleAimStrategy();
    private ITargetStrategy _targetStrategy;

    public void SetWeaponStrategy(IShootStrategy shoot, ShootStrategyType shootType,
        List<IAimStrategy> aims, List<AimStrategyType> aimTypes,
        ITargetStrategy target, TargetStrategyType targetType)
    {
        _shootStrategy = shoot;
        _aimStrategies = aims;
        _targetStrategy = target;

        shootStrategyType = shootType;
        aimStrategyTypes = aimTypes;
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

    public virtual void InitializeFromSO(WeaponSO so, WeaponProgressRO progress)
    {
        Data = so;
        damage = so.baseDamage;
        fireRate = so.baseFireRate;
        health = so.baseHealth;
        attackRadius = so.baseAttackRadius;
        aimThreshold = so.AimThreshold;
        yawTurnSpeed = so.YawTurnSpeed;
        pitchTurnSpeed = so.PitchTurnSpeed;

        baseWeaponEffectChances.AddRange(so.baseEffectChances);

        for (int i = 0; i < progress.DamageLevel; i++)
            damage += so.upgradeLevels[i].damageBonus;
        for (int i = 0; i < progress.FireRateLevel; i++)
            fireRate += so.upgradeLevels[i].fireRateBonus;
        for (int i = 0; i < progress.HealthLevel; i++)
            health += so.upgradeLevels[i].healthBonus;
    }

    protected virtual void Start()
    {
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
        _attachmentPoints = GetComponent<WeaponAttachmentPoints>();

        if (_weaponHitZone != null)
            _weaponHitZone.Initialize(this);

        if (_weaponVisual == null)
            Debug.LogError($"[Weapon] {gameObject.name}  WeaponVisual is null");
        if (_weaponStatusCanvas == null)
            Debug.LogError($"[Weapon] {gameObject.name}  WeaponStatusCanvas is null");
        if (_weaponHitZone == null)
            Debug.LogError($"[Weapon] {gameObject.name}  WeaponHitZone is null");
        if (_attachmentPoints == null)
            Debug.LogError($"[Weapon] {gameObject.name}  _attachmentPoints is null");
    }

    protected virtual void OnDisable()
    {
        if (_target != null)
            _target.OnDeath.RemoveListener(OnTargetDeath);
    }

    protected void Update()
    {
        UpdateTarget();

        if (_target != null && _target.IsAlive)
        {
            _idleTimer = 0f;
            HandleAimingAndShooting();
        }
        else
        {
            HandleIdleAiming();
        }

        if (_fireCountdown > 0f)
            _fireCountdown -= Time.deltaTime;
    }

    private void UpdateTarget()
    {
        _targetRefreshTimer -= Time.deltaTime;
        if (_targetRefreshTimer <= 0f)
        {
            _targetRefreshTimer = _targetRefreshRate;
            _target = _targetStrategy?.FindTarget(this);
        }
    }

    private void HandleAimingAndShooting()
    {
        if (_aimStrategies == null || _aimStrategies.Count == 0)
            return;

        AimResult yawResult = AimResult.Empty;
        AimResult pitchResult = AimResult.Empty;

        foreach (var aim in _aimStrategies)
        {
            if (aim == null) continue;
            var result = aim.ComputeAim(this);
            if (!result.HasAim) continue;

            if (result.ProvidesYaw)
                yawResult = result;

            if (result.ProvidesPitch)
                pitchResult = result;
        }

        bool yawReady = ApplyYaw(yawResult);
        bool pitchReady = ApplyPitch(pitchResult);

        bool aimed = yawReady && pitchReady;

        if (aimed)
        {
            if (_shootStrategy?.TryShoot(this) == true)
            {
                var effectChances = _effectChanceManager.GetAllChancesForWeapon(this);
                var colorWeights = _effectFactory.ConvertChancesToColors(effectChances);
                _weaponVisual?.PlayShotEffect(colorWeights);
            }
        }
    }

    private void HandleIdleAiming()
    {
        _idleTimer += Time.deltaTime;
        if (_idleTimer < idleReturnDelay)
            return;

        var result = _idleAimStrategy.ComputeAim(this);
        if (!result.HasAim)
            return;

        ApplyYaw(result);
        ApplyPitch(result);
    }


    private bool ApplyYaw(AimResult result)
    {
        if (!result.HasAim || !result.ProvidesYaw)
            return false;

        var baseRot = AttachmentPoints.Get(WeaponAttachmentType.BaseRotation);
        if (baseRot == null)
            return false;

        Quaternion targetRot = Quaternion.Euler(0f, result.YawDeg, 0f);
        baseRot.rotation = Quaternion.RotateTowards(
            baseRot.rotation,
            targetRot,
            yawTurnSpeed * Time.deltaTime);

        float yawError = Mathf.Abs(Mathf.DeltaAngle(baseRot.eulerAngles.y, result.YawDeg));
        return yawError <= result.AllowedErrorDeg;
    }

    private bool ApplyPitch(AimResult result)
    {
        if (!result.HasAim || !result.ProvidesPitch)
            return false;

        var barrel = AttachmentPoints.Get(WeaponAttachmentType.BarrelRotation);
        if (barrel == null)
            return false;

        float currentPitch = Mathf.DeltaAngle(0f, barrel.localEulerAngles.x);
        float newPitch = Mathf.MoveTowardsAngle(
            currentPitch,
            result.PitchDeg,
            pitchTurnSpeed * Time.deltaTime);

        barrel.localRotation = Quaternion.Euler(newPitch, 0f, 0f);

        float pitchError = Mathf.Abs(Mathf.DeltaAngle(currentPitch, result.PitchDeg));
        return pitchError <= result.AllowedErrorDeg;
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
            OnDestroyed();
    }

    private void OnDestroyed()
    {
        _target = null;
        _shootStrategy = null;
        _aimStrategies = null;
        _targetStrategy = null;

        foreach (var c in GetComponents<Collider>())
            c.enabled = false;

        OnWeaponDestroyed?.Invoke();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
