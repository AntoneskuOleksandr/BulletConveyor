using System.Collections.Generic;
using UnityEngine;

public interface IBulletMoveStrategy
{
    void Launch(Bullet bullet, Transform target);
}

public interface IBulletHitStrategy
{
    void Hit(Bullet bullet, Collider other);
}

public class StraightMoveStrategy : IBulletMoveStrategy
{
    public void Launch(Bullet bullet, Transform target)
    {
        Vector3 dir = (target.position - bullet.transform.position).normalized;
        bullet.Rigidbody.useGravity = false;
        bullet.Rigidbody.linearVelocity = dir * bullet.Speed;
    }
}

public class DirectHitStrategy : IBulletHitStrategy
{
    public void Hit(Bullet bullet, Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(bullet.Damage);
            bullet.ApplyEffects(enemy);
        }
    }
}

public class ArcMoveStrategy : IBulletMoveStrategy
{
    public void Launch(Bullet bullet, Transform target)
    {
        Vector3 dir = target.position - bullet.transform.position;

        bullet.Rigidbody.useGravity = true;

        float distance = dir.magnitude;
        float angle = 45f * Mathf.Deg2Rad;
        float v = Mathf.Sqrt(distance * Physics.gravity.magnitude / Mathf.Sin(2 * angle));

        Vector3 velocity = dir.normalized * v * Mathf.Cos(angle) + Vector3.up * v * Mathf.Sin(angle);
        bullet.Rigidbody.linearVelocity = velocity;
    }
}

public class AoEHitStrategy : IBulletHitStrategy
{
    private readonly float _radius;

    public AoEHitStrategy(float radius = 5f)
    {
        _radius = radius;
    }

    public void Hit(Bullet bullet, Collider other)
    {
        Collider[] hits = Physics.OverlapSphere(bullet.transform.position, _radius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            var enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(bullet.Damage);
                bullet.ApplyEffects(enemy);
            }
        }
    }
}



public enum BulletType
{
    Standard,
    Artillery
}

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 20f;
    [SerializeField] private float _damage = 10f;

    private Transform _target;
    private Rigidbody _rigidbody;
    private Collider _collider;

    private IBulletMoveStrategy _moveStrategy;
    private IBulletHitStrategy _hitStrategy;

    private readonly List<IOnHitEffect> _effects = new();

    public float Damage => _damage;
    public float Speed => _speed;
    public Transform Target => _target;
    public Rigidbody Rigidbody => _rigidbody;
    public Collider Collider => _collider;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    public void SetStrategies(IBulletMoveStrategy move, IBulletHitStrategy hit)
    {
        _moveStrategy = move;
        _hitStrategy = hit;
    }

    public void AddEffect(IOnHitEffect effect) => _effects.Add(effect);

    public virtual void Init(float damage, Transform spawnPoint, Transform target)
    {
        _damage = damage;
        _target = target;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = false;
        _collider.enabled = true;
        _collider.isTrigger = true;

        _moveStrategy.Launch(this, target);
    }

    private void OnTriggerEnter(Collider other)
    {
        _hitStrategy.Hit(this, other);
        Destroy(gameObject);
    }

    public void ApplyEffects(Enemy enemy)
    {
        foreach (var effect in _effects)
            effect.Apply(this, enemy);
    }
}