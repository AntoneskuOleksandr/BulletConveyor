using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    [Header("Main")]
    public float maxHealth = 1000f;
    public float fadeOutDuration = 2f;
    public float minSpeed = 0.5f;
    public float maxSpeed = 1f;
    public float damage = 5f;

    [Header("Animation")]
    public float deathAnimationDuration = 2.967f;

    [Header("Effects")]

    [Header("Fire")]
    public float burnRadius = 3f;
    public float burnDuration = 5f;
    public float burnDamage = 80f;

    [Header("Freeze")]
    public float freezeRadius = 3f;
    public float freezeDuration = 3f;

    [Header("Poison")]
    public float poisonRadius = 5f;
    public float poisonDuration = 3f;
    public float poisonDamage = 50f;

    [Header("Bool")]
    public bool hasFireInRadiusEffect = false;
    public bool hasFreezeInRadiusEffect = false;
    public bool hasPoisonInRadiusEffect = false;

    [Header("UI")]
    public Image healthBar;
    public GameObject canvas;

    public Gun TargetGun { get; private set; }

    [HideInInspector] public float currentHealth;

    protected LayerMask enemyLayer;
    protected NavMeshAgent agent;
    protected Animator animator;

    protected bool isFrozen = false;
    protected bool isBurning = false;
    protected bool isDead = false;
    protected bool isPoisoned = false;
    protected bool isStunned = false;

    public virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = Random.Range(minSpeed, maxSpeed);
        animator = GetComponent<Animator>();

        currentHealth = maxHealth;

        enemyLayer = this.gameObject.layer;

        GetNewGunPosition();
    }

    public virtual void Update()
    {
        if (isDead)
            return;

        if (agent.remainingDistance > agent.stoppingDistance)
        {
            animator.SetFloat("MoveSpeed", agent.speed);
        }
    }

    public virtual void TakeDamage(float amount)
    {
        if (isDead)
            return;

        currentHealth -= amount;

        canvas.SetActive(true);

        healthBar.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Spawner.Instance.EnemyDied(this);

        if (hasFireInRadiusEffect)
            if (Random.value < AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.FireInRadius))
                BurnEnemiesInRadius(transform.position);

        if (hasFreezeInRadiusEffect)
            if (Random.value < AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.FreezeInRadius))
                FreezeEnemiesInRadius(transform.position);


        if (hasPoisonInRadiusEffect)
            if (Random.value < AbilityProbabilityManager.Instance.GetProbability(AbilityManager.Ability.PoisonInRadius))
                PoisonEnemiesInRadius(transform.position);

        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true;


        tag = "Untagged";
        agent.speed = 0f;
        agent.isStopped = true;
        agent.radius = 0f;
        agent.enabled = false;

        canvas.SetActive(false);
        animator.SetTrigger("DeathTrigger");

        StartCoroutine(FadeOut(3f));
    }

    protected void BurnEnemiesInRadius(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, burnRadius, enemyLayer);

        foreach (Collider collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();

            if (enemy != null && enemy != this)
                enemy.Burn(burnDamage, burnDuration);
        }
    }

    protected void FreezeEnemiesInRadius(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, freezeRadius, enemyLayer);
        foreach (Collider collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && enemy != this)
                enemy.Freeze(freezeDuration);
        }
    }

    protected void PoisonEnemiesInRadius(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, poisonRadius, enemyLayer);
        foreach (Collider collider in colliders)
        {
            Enemy enemy = collider.GetComponent<Enemy>();
            if (enemy != null && enemy != this)
                enemy.Poison(poisonDamage, poisonDuration);
        }
    }

    public void Burn(float totalDamage, float duration)
    {
        if (!isBurning)
        {
            isBurning = true;
            StartCoroutine(BurnOverTime(totalDamage, duration));
        }
    }

    protected IEnumerator BurnOverTime(float totalDamage, float duration)
    {
        float damagePerSecond = totalDamage / duration;
        float rate = 1.0f / duration;
        float progress = 0.0f;

        while (progress < 1.0)
        {
            if (isDead)
            {
                isBurning = false;
                yield break;
            }

            TakeDamage(damagePerSecond * Time.deltaTime);
            progress += rate * Time.deltaTime;
            yield return null;
        }

        isBurning = false;
    }

    public void Freeze(float duration)
    {
        if (!isFrozen)
        {
            isFrozen = true;

            agent.speed *= 0.6f;
            StartCoroutine(Unfreeze(duration));
        }
    }

    protected IEnumerator Unfreeze(float duration)
    {
        yield return new WaitForSeconds(duration);
        isFrozen = false;

        agent.speed /= 0.6f;
    }

    public void Poison(float totalDamage, float duration)
    {
        if (!isPoisoned)
        {
            isPoisoned = true;
            StartCoroutine(PoisonOverTime(totalDamage, duration));
        }
    }

    protected IEnumerator PoisonOverTime(float totalDamage, float duration)
    {
        float damagePerSecond = totalDamage / duration;
        float rate = 1.0f / duration;
        float progress = 0.0f;

        while (progress < 1.0)
        {
            if (isDead)
            {
                isPoisoned = false;
                yield break;
            }

            TakeDamage(damagePerSecond * Time.deltaTime);
            progress += rate * Time.deltaTime;
            yield return null;
        }

        isPoisoned = false;
    }

    public void Stun(float stunDuration)
    {
        if (!isStunned)
        {
            isStunned = true;
            float originalSpeed = agent.speed;
            agent.speed = 0;
            StartCoroutine(RecoverFromStun(stunDuration, originalSpeed));
        }
    }

    protected IEnumerator RecoverFromStun(float duration, float originalSpeed)
    {
        yield return new WaitForSeconds(duration);
        isStunned = false;
        agent.speed = originalSpeed;
    }

    protected IEnumerator FadeOut(float duration)
    {
        yield return new WaitForSeconds(deathAnimationDuration);
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        Material material = skinnedMeshRenderer.material;
        Color color = material.color;
        float rate = 1.0f / duration;
        float progress = 0.0f;

        while (progress < 1.0)
        {
            color.a = Mathf.Lerp(1, 0, progress);
            material.color = color;
            progress += rate * Time.deltaTime;
            yield return null;
        }

        color.a = 0;
        material.color = color;
        Destroy(gameObject);
    }

    protected void OnCollisionEnter(Collision collision)
    {
        Gun gun = collision.gameObject.GetComponent<Gun>();
        if (gun != null)
        {
            gun.TakeDamage(damage);
            Die();
        }
    }

    public void GetNewGunPosition()
    {
        List<Gun> guns = GameManager.Instance.GetAllGuns();
        if (guns.Count > 0)
        {
            TargetGun = guns[Random.Range(0, guns.Count)];
            agent.SetDestination(TargetGun.transform.position);
        }
        else
        {
            TargetGun = null;
            agent.SetDestination(transform.position);
        }
    }
}
