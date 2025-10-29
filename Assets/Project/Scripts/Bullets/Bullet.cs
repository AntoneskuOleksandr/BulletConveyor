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

        // Получаем все доступные эффекты и их шансы
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


    public void Init(Weapon weapon, EffectChanceManager effectChanceManager, EffectFactory effectFactory)
    {
        _weapon = weapon;
        _effectChanceManager = effectChanceManager;
        _effectFactory = effectFactory;

        Damage = weapon.Damage;
        _target = weapon.Target.transform;

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
