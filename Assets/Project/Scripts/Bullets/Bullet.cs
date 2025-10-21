using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
[DisallowMultipleComponent]
public class Bullet : MonoBehaviour
{
    private Transform _target;
    private Rigidbody _rigidbody;
    private Collider _collider;

    private IBulletMoveStrategy _moveStrategy;
    private IBulletHitStrategy _hitStrategy;

    private readonly List<IOnHitEffect> _effects = new();
    private BulletVisual _visual;
    public float Damage { get; private set; }
    public Transform Target => _target;
    public Rigidbody Rigidbody => _rigidbody;
    public Collider Collider => _collider;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _visual = GetComponent<BulletVisual>();
    }

    private void FixedUpdate()
    {
        _moveStrategy?.UpdateRotation(this);
    }


    public void SetStrategies(IBulletMoveStrategy move, IBulletHitStrategy hit)
    {
        _moveStrategy = move;
        _hitStrategy = hit;
    }

    public void AddEffect(IOnHitEffect effect) => _effects.Add(effect);

    public void ApplyEffects(Enemy enemy)
    {
        foreach (var effect in _effects)
            effect.Apply(this, enemy);
    }

    public void Init(float damage, float launchSpeed, Transform spawnPoint, Transform target)
    {
        Damage = damage;
        _target = target;

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;

        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = false;
        _collider.enabled = true;
        _collider.isTrigger = true;

        _visual?.OnSpawn();
        SetVisual(Color.green);

        _moveStrategy?.Launch(this, target, launchSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        _hitStrategy?.Hit(this, other);
        _visual?.OnDespawn();
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        _visual?.OnDespawn();
    }

    public void SetVisual(Color baseColor)
    {
        _visual?.SetBaseColor(baseColor);
    }
}
