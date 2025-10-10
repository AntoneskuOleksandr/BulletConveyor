using System.Collections.Generic;
using UnityEngine;

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

    public void Init(float damage, Transform spawnPoint, Transform target)
    {
        _damage = damage;
        _target = target;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = false;
        _collider.enabled = true;
        _collider.isTrigger = true;

        _moveStrategy?.Launch(this, target);
    }

    private void OnTriggerEnter(Collider other)
    {
        _hitStrategy?.Hit(this, other);
        Destroy(gameObject);
    }

    public void ApplyEffects(Enemy enemy)
    {
        foreach (var effect in _effects)
            effect.Apply(this, enemy);
    }
}
