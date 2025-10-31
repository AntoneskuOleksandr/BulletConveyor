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
    private float _damage;
    private float _health;
    private float _fireRate;
    private float _maxAttackRadius;
    private float _minAttackRadius;
    private float _aimThreshold;
    private float _idleReturnDelay = 2f;

    private bool _useYaw;
    private bool _usePitch;
    private float _yawTurnSpeed;
    private float _pitchTurnSpeed;

    public WeaponSO Data { get; private set; }
    public float Damage => _damage;
    public float Health => _health;
    public float FireRate => _fireRate;
    public float FireCountdown => _fireCountdown;
    public float MaxAttackRadius => _maxAttackRadius;
    public float MinAttackRadius => _minAttackRadius;
    public float FindTargetRadius => _maxAttackRadius * Data.FindTargetRadiusCoefficient;
    public float LaunchSpeed => CalculateLaunchSpeed();
    public float AimThreshold => _aimThreshold;
    public Enemy Target => _target;
    public WeaponAttachmentPoints AttachmentPoints => _attachmentPoints;
    public Action OnWeaponDestroyed;

    private List<WeaponEffectChance> baseWeaponEffectChances = new();
    private WeaponAttachmentPoints _attachmentPoints;

    [Inject] private EffectChanceManager _effectChanceManager;
    [Inject] private EffectFactory _effectFactory;

    private WeaponVisual _weaponVisual;
    private WeaponStatusCanvas _weaponStatusCanvas;
    private WeaponHitZone _weaponHitZone;

    private Enemy _target;

    private float _fireCountdown;

    private float _targetRefreshRate = 1f;
    private float _targetRefreshTimer = 0f;

    private float _idleTimer = 0f;

    [Header("Strategy Info (Read Only at inspector)")]
    [SerializeField] private List<AimStrategyType> aimStrategyTypes;
    [SerializeField] private ShootStrategyType shootStrategyType;
    [SerializeField] private TargetStrategyType targetStrategyType;
    [SerializeField] private BulletMoveType bulletMoveType;
    [SerializeField] private BulletHitType bulletHitType;

    [Header("Weapon Modifiers (Read Only at inspector)")]
    [SerializeField] private List<WeaponModifierType> weaponModifierTypes;

    public IBulletMoveStrategy BulletMoveStrategy => _bulletMoveStrategy;
    public IBulletHitStrategy BulletHitStrategy => _bulletHitStrategy;

    private IBulletMoveStrategy _bulletMoveStrategy;
    private IBulletHitStrategy _bulletHitStrategy;

    private List<IAimStrategy> _aimStrategies = new();
    private IdleAimStrategy _idleAimStrategy = new IdleAimStrategy();

    private IShootStrategy _shootStrategy;
    private ITargetStrategy _targetStrategy;


    private List<IWeaponModifier> _weaponModifiers = new();

    public void SetWeaponStrategy(
        List<IAimStrategy> aims, List<AimStrategyType> aimTypes,
        IShootStrategy shoot, ShootStrategyType shootType,
        ITargetStrategy target, TargetStrategyType targetType)
    {
        _aimStrategies = aims;
        _shootStrategy = shoot;
        _targetStrategy = target;

        aimStrategyTypes = aimTypes;
        shootStrategyType = shootType;
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

    public void SetWeaponModifiers(List<IWeaponModifier> modifiers, List<WeaponModifierType> weaponModifierTypes)
    {
        _weaponModifiers = modifiers;
        this.weaponModifierTypes = weaponModifierTypes;
    }

    public virtual void InitializeFromSO(WeaponSO so, WeaponProgressRO progress)
    {
        Data = so;
        _damage = so.BaseDamage;
        _fireRate = so.BaseFireRate;
        _health = so.BaseHealth;
        _maxAttackRadius = so.MaxAttackRadius;
        _minAttackRadius = so.MinAttackRadius;
        _aimThreshold = so.AimThreshold;
        _yawTurnSpeed = so.YawTurnSpeed;
        _pitchTurnSpeed = so.PitchTurnSpeed;
        _useYaw = so.UseYaw;
        _usePitch = so.UsePitch;

        baseWeaponEffectChances.AddRange(so.BaseEffectChances);

        for (int i = 0; i < progress.DamageLevel; i++)
            _damage += so.UpgradeLevels[i].DamageBonus;
        for (int i = 0; i < progress.FireRateLevel; i++)
            _fireRate += so.UpgradeLevels[i].FireRateBonus;
        for (int i = 0; i < progress.HealthLevel; i++)
            _health += so.UpgradeLevels[i].HealthBonus;

        foreach (var modifier in _weaponModifiers)
            modifier.Initialize(this);
    }

    public BulletInitData BuildBulletInitData()
    {
        BulletInitData data = new BulletInitData
        {
            Damage = _damage,
            LaunchSpeed = LaunchSpeed,
            HasOverrideForward = false,
            OverrideForward = Vector3.zero,
        };

        foreach (var m in _weaponModifiers)
            m.OnBeforeBulletInit(this, ref data);

        return data;
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

    public void NotifyBeforeShoot()
    {
        foreach (var m in _weaponModifiers)
            m.OnBeforeShoot(this);
    }

    public void NotifyAfterShoot(List<Bullet> bullets)
    {
        foreach (var m in _weaponModifiers)
            m.OnAfterShoot(this, bullets);
    }

    public void NotifyBulletInit(Bullet bullet)
    {
        foreach (var m in _weaponModifiers)
            m.OnBulletInit(this, bullet);
    }

    public void NotifyBulletHit(Bullet bullet, Collider hit)
    {
        foreach (var m in _weaponModifiers)
            m.OnBulletHit(this, bullet, hit);
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

        float distance = Vector3.Distance(_target.transform.position, transform.position);
        if (distance < MinAttackRadius || distance > FindTargetRadius)
            return;

        foreach (var aim in _aimStrategies)
        {
            if (aim == null) continue;
            var result = aim.ComputeAim(this);
            if (!result.HasAim) continue;

            if (result.ProvidesYaw) yawResult = result;
            if (result.ProvidesPitch) pitchResult = result;
        }

        bool yawReady = !_useYaw || ApplyYaw(yawResult);
        bool pitchReady = !_usePitch || ApplyPitch(pitchResult);
        bool aimed = yawReady && pitchReady;

        if (!aimed)
            return;

        bool canShoot = _shootStrategy.IsCanShoot(this);

        if (canShoot)
        {
            NotifyBeforeShoot();

            var firedBullets = _shootStrategy.Shoot(this);

            var effectChances = _effectChanceManager.GetAllChancesForWeapon(this);
            var colorWeights = _effectFactory.ConvertChancesToColors(effectChances);
            _weaponVisual?.PlayShotEffect(colorWeights);

            NotifyAfterShoot(firedBullets);
        }
    }


    private void HandleIdleAiming()
    {
        _idleTimer += Time.deltaTime;
        if (_idleTimer < _idleReturnDelay)
            return;

        var result = _idleAimStrategy.ComputeAim(this);
        if (!result.HasAim)
            return;

        if (_useYaw && result.ProvidesYaw)
            ApplyYaw(result);

        if (_usePitch && result.ProvidesPitch)
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

        float smoothFactor = 1f - Mathf.Exp(-_yawTurnSpeed * Time.deltaTime);
        baseRot.rotation = Quaternion.Slerp(baseRot.rotation, targetRot, smoothFactor);

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
        float smoothFactor = 1f - Mathf.Exp(-_pitchTurnSpeed * Time.deltaTime);
        float newPitch = Mathf.LerpAngle(currentPitch, result.PitchDeg, smoothFactor);

        barrel.localRotation = Quaternion.Euler(newPitch, 0f, 0f);

        float pitchError = Mathf.Abs(Mathf.DeltaAngle(currentPitch, result.PitchDeg));
        return pitchError <= result.AllowedErrorDeg;
    }


    public void ResetFireCountdown()
    {
        _fireCountdown = 1f / _fireRate;
    }

    public void OnTargetDeath()
    {
        _target = null;
    }

    private float CalculateLaunchSpeed()
    {
        switch (Data.LaunchSpeedMode)
        {
            case LaunchSpeedMode.AutoFromRadius:
                {
                    Vector3 fromWeaponToFirePoint = _attachmentPoints.Get(WeaponAttachmentType.FirePoint).position - transform.position;
                    return Mathf.Sqrt((_maxAttackRadius - fromWeaponToFirePoint.magnitude) * Mathf.Abs(Physics.gravity.y));
                }
            case LaunchSpeedMode.Fixed:
            default:
                return Data.LaunchSpeed;
        }
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        _health -= amount;
        _weaponStatusCanvas?.UpdateHealthBar(_health, Data.BaseHealth);

        if (_health <= 0f)
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

    protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _maxAttackRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Data.MinAttackRadius);
    }
}
