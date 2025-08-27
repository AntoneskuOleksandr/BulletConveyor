using UnityEngine;
using Zenject;

public enum BulletType
{
    Standard,
    Missile
}

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] private BulletType bulletType;
    public BulletType BulletType => bulletType;

    [Header("Weapon Settings")]
    public float damage = 10f;
    public Transform firePoint;
    public Transform partToRotate;
    public float turnSpeed = 3f;           // скорость поворота ствола
    public float fireRate = 1f;            // выстрелов в секунду
    public float aimThreshold = 2f;        // допустимый угол наведения (в градусах)
    public float attackRadius = 50f;       // радиус действия башни
    public float idleReturnDelay = 2f;     // через сколько секунд без цели возвращаться в исходное положение

    protected Bullet _bulletInAmmo;
    protected BulletManager _bulletManager;
    protected EnemyManager _enemyManager;

    protected Enemy target;
    protected float _fireCountdown = 0f;

    protected float _targetRefreshRate = 1f;
    protected float _targetRefreshTimer = 0f;

    private Quaternion _initialRotation;
    private float _idleTimer = 0f;

    [Inject]
    public void Construct(BulletManager bulletManager, EnemyManager enemyManager)
    {
        _bulletManager = bulletManager;
        _enemyManager = enemyManager;
    }

    protected virtual void Start()
    {
        if (partToRotate != null)
            _initialRotation = partToRotate.rotation;
    }

    protected virtual void OnDisable()
    {
        if (target != null)
            target.OnDeath.RemoveListener(OnTargetDeath);
    }

    protected void Update()
    {
        // Периодический поиск цели
        _targetRefreshTimer -= Time.deltaTime;
        if (_targetRefreshTimer <= 0f)
        {
            _targetRefreshTimer = _targetRefreshRate;
            UpdateTarget();
        }

        if (target != null)
        {
            _idleTimer = 0f;

            // Всегда смотрим на цель, даже если пуль нет
            LockOnTarget();

            // Стреляем только если есть пуля и наведены
            if (_bulletInAmmo == null)
                _bulletInAmmo = _bulletManager.GetBullet(this);

            if (_bulletInAmmo != null && IsAimedAtTarget() && _fireCountdown <= 0f)
            {
                TryToShoot();
                _fireCountdown = 1f / fireRate;
                _bulletInAmmo = null;
            }
        }
        else
        {
            HandleIdleRotation();
        }

        // Отсчёт до следующего выстрела
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
    }

    protected void LockOnTarget()
    {
        if (target == null)
            return;

        Vector3 dir = target.HitTransform.position - partToRotate.position;

        dir.y = 0f;

        if (dir == Vector3.zero)
            return;

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
        if (target == null)
            return false;

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
