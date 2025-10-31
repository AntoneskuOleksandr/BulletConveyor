using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
[DisallowMultipleComponent]
public class Bullet : MonoBehaviour
{
    public float Damage { get; private set; }
    public Transform Target => _target;
    public Rigidbody Rigidbody => _rigidbody;
    public Collider Collider => _collider;

    private Weapon _weapon;
    private Transform _target;
    private Rigidbody _rigidbody;
    private Collider _collider;

    private EffectChanceManager _effectChanceManager;
    private EffectFactory _effectFactory;

    private IBulletMoveStrategy _moveStrategy;
    private IBulletHitStrategy _hitStrategy;

    private readonly List<IOnHitEffect> _effects = new();
    private BulletVisual _visual;

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
        if (_weapon == null) return;

        var allChances = _effectChanceManager.GetAllChancesForWeapon(_weapon);

        foreach (var pair in allChances)
        {
            var effectType = pair.Key;
            float chance = pair.Value;

            if (Random.value <= chance)
            {
                var effect = _effectFactory.CreateEffect(effectType);
                effect?.Apply(this, enemy);
            }
        }
    }

    public void Init(Weapon weapon, EffectChanceManager chanceManager, EffectFactory effectFactory,
    Transform spawnTransform, BulletInitData data)
    {
        _weapon = weapon;
        _effectChanceManager = chanceManager;
        _effectFactory = effectFactory;

        Damage = data.Damage;
        _target = weapon.Target.transform;

        transform.position = spawnTransform.position;

        if (data.HasOverrideForward)
            transform.rotation = Quaternion.LookRotation(data.OverrideForward, Vector3.up);
        else
            transform.rotation = spawnTransform.rotation;

        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = false;
        _collider.enabled = true;
        _collider.isTrigger = true;
        gameObject.isStatic = false;

        _visual?.OnSpawn();
        SetVisual(Color.green);

        _moveStrategy?.Launch(this, _target, weapon.LaunchSpeed);
    }


    private void OnTriggerEnter(Collider other)
    {
        _hitStrategy?.Hit(_weapon, this, other);
        _weapon.NotifyBulletHit(this, other);
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
