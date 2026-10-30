using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Zenject;

public abstract class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] protected float baseSpeed = 1f;
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] private float killReward = 10f;
    [SerializeField] private float gearsKillReward = 1f;
    [SerializeField] private float contactDamage = 20f;
    [SerializeField] protected Transform hitTransform;
    [SerializeField] protected EnemyStatusCanvas enemyStatusCanvas;
    [Header("Read-Only")]
    [SerializeField] protected float _currentHealth;

    protected Transform _target;
    protected NavMeshAgent _agent;
    protected Animator _animator;
    protected Collider _collider;
    protected Rigidbody _rigidbody;

    private WeaponManager weaponManager;
    private float targetRefresh;
    public float BodyRadius { get; private set; }

    public float ContactDamage => contactDamage;
    public bool IsAlive => _currentHealth > 0f;
    public float Health => _currentHealth;
    public float BaseSpeed => baseSpeed;
    public float CoinsKillReward => killReward;
    public float GearsKillReward => gearsKillReward;
    public Transform HitTransform => hitTransform;
    public UnityEvent OnDeath;

    protected Dictionary<OnHitEffectType, IStatusEffect> _effects = new();

    public ModifierManager ModifierManager { get; private set; }

    [Inject]
    public void Construct(WeaponManager weaponManager, DiContainer container)
    {

        this.weaponManager = weaponManager;
        ModifierManager = container.InstantiateComponent<ModifierManager>(this.gameObject);
        ModifierManager.Initialize(this, enemyStatusCanvas);

        _target = weaponManager.GetRandomWeapon()?.transform;

        if (_target == null)
            Debug.LogError("[Enemy] There are no weapons at scene.");

        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();

        BodyRadius = EnemySpawner.GetRadius(gameObject);
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
        }
        if (_animator != null) _animator.applyRootMotion = false;
        _agent.speed = baseSpeed;
        _agent.acceleration = Mathf.Max(2f, baseSpeed * 3f);
        _agent.avoidancePriority = Random.Range(30, 70);
        _currentHealth = maxHealth;
    }

    protected void OnEnable()
    {
        OnDeath.AddListener(HideStatus);

        if (_target != null && _agent != null && _agent.isOnNavMesh)
        {
            _agent.SetDestination(_target.position);
            _animator.SetFloat("MoveSpeed", _agent.speed / 2f);
        }
    }

    protected void OnDisable()
    {
        OnDeath.RemoveListener(HideStatus);
    }

    private void HideStatus() => enemyStatusCanvas.HideStatusCanvas();

    private void Update()
    {
        if (!IsAlive || _agent == null || !_agent.enabled || !_agent.isOnNavMesh) return;
        targetRefresh -= Time.deltaTime;
        if (targetRefresh > 0f) return;
        targetRefresh = 0.5f;
        var weapon = _target != null ? _target.GetComponent<Weapon>() : null;
        if (weapon == null || !weapon.IsAlive) _target = weaponManager.GetRandomWeapon()?.transform;
        if (_target != null) _agent.SetDestination(_target.position);
    }

    public void SetSpeed(float speed)
    {
        _agent.speed = speed;
        _animator.SetFloat("MoveSpeed", speed / 2f);
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        _currentHealth -= damage;
        enemyStatusCanvas.SetHealthBarFillAmount(_currentHealth / maxHealth);

        if (!IsAlive)
            Die();
    }

    protected void Die()
    {
        _currentHealth = 0f;
        GameAudio.PlayEnemyDeath();
        OnDeath?.Invoke();

        _collider.enabled = false;
        if (_rigidbody != null) _rigidbody.isKinematic = true;

        if (_agent != null)
        {
            if (_agent.isOnNavMesh) _agent.isStopped = true;
            _agent.enabled = false;
        }

        if (_animator != null)
            _animator.SetTrigger("DeathTrigger");

        Destroy(this.gameObject, 5f);
    }
}
