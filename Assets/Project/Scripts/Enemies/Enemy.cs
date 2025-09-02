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
    [SerializeField] protected Transform hitTransform;
    [SerializeField] protected EnemyStatusCanvas enemyStatusCanvas;

    protected Transform _target;
    protected NavMeshAgent _agent;
    protected Animator _animator;
    protected Collider _collider;
    protected Rigidbody _rigidbody;

    protected float _currentHealth;

    public bool IsAlive => _currentHealth > 0f;
    public float BaseSpeed => baseSpeed;
    public Transform HitTransform => hitTransform;
    public UnityEvent OnDeath;

    // Все активные дебафы/бафы на враге
    protected Dictionary<OnHitEffectType, IStatusEffect> _effects = new();

    // Менеджер модификаторов для комбинирования эффектов
    public ModifierManager ModifierManager { get; private set; }

    [Inject]
    public void Construct(WeaponManager weaponManager)
    {
        // Добавляем менеджер модификаторов на врага
        ModifierManager = this.gameObject.AddComponent<ModifierManager>();
        ModifierManager.Initialize(this, enemyStatusCanvas);

        _target = weaponManager.GetRandomWeapon()?.transform;

        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();

        _currentHealth = maxHealth;
    }

    protected void OnEnable()
    {
        OnDeath.AddListener(() => enemyStatusCanvas.HideHealthBar());

        if (_target != null && _agent != null)
        {
            _agent.SetDestination(_target.position);
            _animator.SetFloat("MoveSpeed", _agent.speed / 2f);
        }
    }

    protected void OnDisable()
    {
        OnDeath.RemoveListener(() => enemyStatusCanvas.HideHealthBar());
    }

    public void SetSpeed(float speed)
    {
        _animator.SetFloat("MoveSpeed", _agent.speed / 2f);
        _agent.speed = speed;
    }

    public void TakeDamage(float damage)
    {
        if (!IsAlive) return;

        _currentHealth -= damage;
        enemyStatusCanvas.SetHealthBarFillAmount(_currentHealth / maxHealth);

        if (!IsAlive)
            Die();
    }

    /// <summary>
    /// Смерть врага
    /// </summary>
    protected void Die()
    {
        _currentHealth = 0f;
        OnDeath?.Invoke();

        _collider.enabled = false;
        if (_rigidbody != null) _rigidbody.isKinematic = true;

        if (_agent != null)
        {
            _agent.isStopped = true;
            _agent.enabled = false;
        }

        if (_animator != null)
            _animator.SetTrigger("DeathTrigger");

        Destroy(this.gameObject, 5f);
    }
}
