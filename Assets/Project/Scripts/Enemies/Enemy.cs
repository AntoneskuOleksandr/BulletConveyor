using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using Zenject;

public abstract class Enemy : MonoBehaviour
{
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

    public Transform HitTransform => hitTransform;
    public UnityEvent OnDeath;

    protected Dictionary<OnHitEffectType, IStatusEffect> _effects = new();

    [Inject]
    public void Construct(WeaponManager weaponManager)
    {
        _target = weaponManager.GetRandomWeapon().transform;

        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();

        _currentHealth = maxHealth;
    }

    protected void OnEnable()
    {
        OnDeath.AddListener(() => enemyStatusCanvas.HideHealthBar());

        _agent.SetDestination(_target.position);
        _animator.SetFloat("MoveSpeed", _agent.speed / 2f);
    }

    protected void OnDisable()
    {
        OnDeath.RemoveListener(() => enemyStatusCanvas.HideHealthBar());
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        enemyStatusCanvas.SetHealthBarFillAmount(_currentHealth / maxHealth);

        if (_currentHealth <= 0f)
        {
            Die();
        }
    }

    public void AddStatusEffect(IStatusEffect effect)
    {
        if (_effects.TryGetValue(effect.EffectType, out var existing))
        {
            existing.Refresh(effect);
        }
        else
        {
            _effects[effect.EffectType] = effect;
            effect.Start(this);
        }
    }

    public void RemoveStatusEffect(OnHitEffectType type)
    {
        if (_effects.ContainsKey(type))
        {
            _effects.Remove(type);
        }
    }

    protected void Die()
    {
        OnDeath?.Invoke();

        _collider.enabled = false;
        _rigidbody.isKinematic = true;

        _agent.isStopped = true;
        _agent.enabled = false;

        _animator.SetTrigger("DeathTrigger");

        Destroy(this.gameObject, 5f);
    }
}
