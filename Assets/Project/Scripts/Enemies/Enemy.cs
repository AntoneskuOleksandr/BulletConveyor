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

    protected float _currentHealth;

    public bool IsAlive => _currentHealth > 0f;

    public Transform HitTransform => hitTransform;
    public UnityEvent OnDeath;


    [Inject]
    public void Construct(WeaponManager weaponManager)
    {
        _target = weaponManager.GetRandomWeapon().transform;

        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _currentHealth = maxHealth;
        _collider = GetComponent<Collider>();
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

    protected void Die()
    {
        OnDeath?.Invoke();

        _animator.SetTrigger("DeathTrigger");
        _agent.isStopped = true;

        _collider.enabled = false;

        Destroy(this.gameObject, 5f);
    }
}
