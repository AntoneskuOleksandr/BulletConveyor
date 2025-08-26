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

    protected Transform target;
    protected float _fireCountdown = 0f;

    protected float _targetRefreshRate = 0.25f;
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
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist < attackRadius)
                return;
        }
        target = _enemyManager.GetNearestEnemy(this, attackRadius);
    }

    protected virtual void TryToShoot()
    {
        if (_bulletInAmmo == null || target == null)
            return;

        _bulletInAmmo.Init(damage, firePoint, target);
    }

    protected void LockOnTarget()
    {
        if (target == null)
            return;

        Vector3 dir = (target.position - partToRotate.position).normalized;
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

        Vector3 dirToTarget = (target.position - partToRotate.position).normalized;
        float angle = Vector3.Angle(partToRotate.forward, dirToTarget);
        return angle < aimThreshold;
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
