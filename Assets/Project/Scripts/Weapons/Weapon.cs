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

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] private BulletType bulletType;
    public BulletType BulletType => bulletType;

    [Header("Weapon Settings")]
    public float damage = 10f;
    public Transform firePoint;
    public Transform partToRotate;
    public float turnSpeed = 3f;
    public float fireRate = 1f;
    public float aimThreshold = 2f;
    public float attackRadius = 50f;
    public float idleReturnDelay = 2f;
    public WeaponSO Data { get; private set; }

    [Header("Effect Chances from Weapon")]
    [Tooltip("Задайте стартовые шансы на разные дебаффы от этой пушки.")]
    public List<WeaponEffectChance> effectChances = new List<WeaponEffectChance>();

    protected Bullet _bulletInAmmo;
    protected BulletManager _bulletManager;
    protected EnemyManager _enemyManager;
    protected EffectChanceManager _effectChanceManager;

    protected Enemy target;
    protected float _fireCountdown = 0f;

    protected float _targetRefreshRate = 1f;
    protected float _targetRefreshTimer = 0f;

    private Quaternion _initialRotation;
    private float _idleTimer = 0f;

    [Inject]
    public void Construct(BulletManager bulletManager, EnemyManager enemyManager, EffectChanceManager effectChanceManager)
    {
        _bulletManager = bulletManager;
        _enemyManager = enemyManager;
        _effectChanceManager = effectChanceManager;
    }
    public virtual void InitializeFromSO(WeaponSO so)
    {
        Data = so;
        damage = so.baseDamage;
        fireRate = so.baseFireRate;

        var attachment = GetComponent<WeaponAttachmentPoints>();
        if (attachment != null)
        {
            firePoint = attachment?.firePoint;
            partToRotate = attachment?.partToRotate;
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
        _targetRefreshTimer -= Time.deltaTime;
        if (_targetRefreshTimer <= 0f)
        {
            _targetRefreshTimer = _targetRefreshRate;
            UpdateTarget();
        }

        if (target != null)
        {
            _idleTimer = 0f;
            LockOnTarget();

            if (_bulletInAmmo == null && IsAimedAtTarget() && _fireCountdown <= 0f)
            {
                _bulletInAmmo = _bulletManager.GetBullet(this);
                TryToShoot();
            }
        }
        else
        {
            HandleIdleRotation();
        }

        if (_fireCountdown > 0f)
            _fireCountdown -= Time.deltaTime;
    }

    protected void UpdateTarget()
    {
        if (target != null)
            target.OnDeath.RemoveListener(OnTargetDeath);

        target = _enemyManager.GetNearestEnemy(this, attackRadius);

        if (target != null)
            target.OnDeath.AddListener(OnTargetDeath);
    }

    protected virtual void TryToShoot()
    {
        if (_bulletInAmmo == null || target == null)
            return;

        _bulletInAmmo.Init(damage, firePoint, target.HitTransform);

        _fireCountdown = 1f / fireRate;
        _bulletInAmmo = null;
    }

    protected void LockOnTarget()
    {
        if (target == null)
            return;

        Vector3 dir = target.HitTransform.position - partToRotate.position;
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(dir);
        partToRotate.rotation = Quaternion.Lerp(
            partToRotate.rotation,
            lookRotation,
            Time.deltaTime * turnSpeed
        );
    }

    private void HandleIdleRotation()
    {
        _idleTimer += Time.deltaTime;
        if (_idleTimer >= idleReturnDelay)
        {
            partToRotate.rotation = Quaternion.Lerp(
                partToRotate.rotation,
                _initialRotation,
                Time.deltaTime * turnSpeed
            );
        }
    }

    protected bool IsAimedAtTarget()
    {
        if (target == null) return false;

        Vector3 dirToTarget = target.HitTransform.position - partToRotate.position;
        dirToTarget.y = 0f;
        dirToTarget.Normalize();

        Vector3 forward = partToRotate.forward;
        forward.y = 0f;
        forward.Normalize();

        float angle = Vector3.Angle(forward, dirToTarget);
        return angle < aimThreshold;
    }

    protected void OnTargetDeath()
    {
        UpdateTarget();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
