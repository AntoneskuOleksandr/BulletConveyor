using System;
using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    [Header("General")]
    public float speed = 70f;
    public float explosionRadius = 5f;
    public float yEnemyOffSet = 1.2f;
    public LayerMask enemyLayer;
    public float timeToDead = 3f;

    [Header("Effects")]
    [Header("Burn")]
    public float burnRadius;
    public float burnDuration;
    public float burnDamage;

    [Header("Freeze")]
    public float freezeRadius;
    public float freezeDuration;

    [Header("Poison")]
    public float poisonRadius;
    public float poisonDuration;
    public float poisonDamage;

    [Header("Stun")]
    public float stunDuration;

    [Header("Bullet Damage Increase")]
    public float damageMultiplier;

    public GameObject impactEffect;
    public enum Effect { None, Burn, Freeze, InstantDeath, Poison, Stun, IncearedDamage, Explosion }
    public Effect effect = Effect.None;

    public TrailRenderer trail;
    public Color trailColor;

    public float damage;

    public event Action<GameObject> OnBulletHit;


    private void Start()
    {
        burnDamage = damage;
        poisonDamage = damage;
    }

    private void OnEnable()
    {
        UpdateTrailColor();
    }

    public void UpdateTrailColor()
    {
        trail.startColor = trailColor;
        trail.endColor = trailColor;
        trail.enabled = true;
    }

    public virtual void HitTarget(Transform target)
    {
        Enemy enemy = target.GetComponent<Enemy>();

        Debug.Log(effect);

        if (effect == Effect.Burn)
            enemy.Burn(burnDamage, burnDuration);
        else if (effect == Effect.Freeze)
            enemy.Freeze(freezeDuration);
        else if (effect == Effect.InstantDeath)
            enemy.Die();
        else if (effect == Effect.Poison)
            enemy.Poison(poisonDamage, poisonDuration);
        else if (effect == Effect.Stun)
            enemy.Stun(stunDuration);
        else if (effect == Effect.IncearedDamage)
            damage *= damageMultiplier;
        else if (effect == Effect.Explosion)
            Explode();

        Damage(target);

        if (enemy.hasFireInRadiusEffect)
        {
            enemy.burnRadius = burnRadius;
            enemy.burnDuration = burnDuration;
            enemy.burnDamage = burnDamage;
        }
        else if (enemy.hasFreezeInRadiusEffect)
        {
            enemy.freezeRadius = freezeRadius;
            enemy.freezeDuration = freezeDuration;
        }
        else if (enemy.hasPoisonInRadiusEffect)
        {
            enemy.poisonRadius = poisonRadius;
            enemy.poisonDuration = poisonDuration;
            enemy.poisonDamage = poisonDamage;
        }
    }

    public void ShowParticles()
    {
        GameObject effectIns = Instantiate(impactEffect, transform.position, transform.rotation);

        Material newMat = new Material(effectIns.GetComponent<ParticleSystemRenderer>().material.shader);
        newMat.color = trailColor;

        effectIns.GetComponent<ParticleSystemRenderer>().material = newMat;

        Destroy(effectIns, 3f);
    }
    public void Damage(Transform enemyTransform)
    {
        Enemy enemy = enemyTransform.GetComponent<Enemy>();
        if (enemy != null)
            enemy.TakeDamage(damage);
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (this.GetComponent<Bullet>().enabled)
        {
            if (other.CompareTag("Enemy"))
            {
                HitTarget(other.transform);
                ShowParticles();
            }

            OnBulletHit?.Invoke(gameObject);
        }
    }

    public void SetOnWaitingState()
    {
        gameObject.SetActive(false);
        gameObject.isStatic = false;
    }

    public void Explode()
    {
        Collider[] colliders = new Collider[20];
        int count = Physics.OverlapSphereNonAlloc(transform.position, explosionRadius, colliders, enemyLayer);

        for (int i = 0; i < count; i++)
        {
            Enemy enemy = colliders[i].GetComponent<Enemy>();

            if (enemy.hasFireInRadiusEffect)
            {
                enemy.burnRadius = burnRadius;
                enemy.burnDuration = burnDuration;
                enemy.burnDamage = burnDamage;
            }
            else if (enemy.hasFreezeInRadiusEffect)
            {
                enemy.freezeRadius = freezeRadius;
                enemy.freezeDuration = freezeDuration;
            }
            else if (enemy.hasPoisonInRadiusEffect)
            {
                enemy.poisonRadius = poisonRadius;
                enemy.poisonDuration = poisonDuration;
                enemy.poisonDamage = poisonDamage;
            }

            enemy.TakeDamage(damage);
        }
        ShowParticles();
        gameObject.SetActive(false);
    }
}
