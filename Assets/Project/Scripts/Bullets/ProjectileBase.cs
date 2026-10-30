using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ProjectileBase : MonoBehaviour
{
    private Weapon _weapon;
    public Rigidbody Rigidbody { get; private set; }
    public Collider Collider { get; private set; }
    public ProjectileInitData Data { get; private set; }
    public float Damage => Data.Damage;

    private IProjectileMoveStrategy _moveStrategy;
    private IProjectileHitStrategy _hitStrategy;
    private EffectChanceManager _effectChanceManager;
    private EffectFactory _effectFactory;
    private bool _isActive;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
        Collider = GetComponent<Collider>();
        Collider.isTrigger = true;
    }

    public void Init(
        Weapon weapon,
        ProjectileInitData data,
        IProjectileMoveStrategy move,
        IProjectileHitStrategy hit,
        EffectChanceManager chanceManager,
        EffectFactory effectFactory)
    {
        this._weapon = weapon;
        Data = data;
        _moveStrategy = move;
        _hitStrategy = hit;
        _effectChanceManager = chanceManager;
        _effectFactory = effectFactory;
        _isActive = true;
        gameObject.isStatic = false;
        Destroy(gameObject, 12f);

        if (data.HasOverrideForward)
            transform.rotation = Quaternion.LookRotation(data.OverrideForward);
    }

    private void Start()
    {
        _moveStrategy?.Launch(_weapon, this, Data);
    }

    private void FixedUpdate()
    {
        if (_isActive)
            _moveStrategy?.Update(_weapon, this, Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_isActive || other.gameObject.layer == LayerMask.NameToLayer("Weapon") ||
            other.gameObject.layer == LayerMask.NameToLayer("Bullet") || other.isTrigger)
            return;

        _isActive = false;
        _hitStrategy?.Hit(_weapon, this, other);
        _weapon.NotifyProjectileHit(this, other);
        Destroy(gameObject);
    }

    public void ApplyEffects(Enemy enemy)
    {
        var allChances = _effectChanceManager.GetAllChancesForWeapon(_weapon);
        foreach (var pair in allChances)
        {
            if (Random.value <= pair.Value)
            {
                var effect = _effectFactory.CreateEffect(pair.Key);
                effect?.Apply(this, enemy);
            }
        }
    }
}
