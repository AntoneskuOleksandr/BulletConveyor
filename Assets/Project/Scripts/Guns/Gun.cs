using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Gun : MonoBehaviour
{
    [Header("General")]
    public float maxHealth = 20f;

    [Header("Use Bullets (default)")]
    public float fireRate = 1f;
    public bool needBullet = true;

    [Header("Unity Setup Fields")]
    public string enemyTag = "Enemy";
    public int gunID;
    public Transform partToRotate;
    public float turnSpeed = 3f;
    public float rotationSpeed = 3f;

    [Header("Bullet")]
    public GameObject bulletPrefab;
    public GameObject bulletInAmmoGO;
    public Transform firePoint;
    public float yEnemyOffSet = 1.8f;
    public float bulletSpeed = 50f;
    protected Color bulletColor;

    [Header("Effects")]
    public GameObject firingEffect;
    public enum PasiveAbility { None, FireBullet, FreezeBullet, DeadBullet, PoisonBullet, StunBullet, DamageIncearedBullet, ExlosionBullet }
    public PasiveAbility pasiveAbility = PasiveAbility.None;

    [Header("UI")]
    public Image healthBar;
    public GameObject healthCanvas;

    protected Bullet bulletInAmmo;
    protected StackBullets stackBullets;
    protected AbilityProbabilityManager probabilityManager;
    protected AbilityManager abilityManager;
    protected Transform target;
    protected Rigidbody bulletRb;
    protected Vector3 direction;
    protected Material newMaterial;

    protected float fireCountDown = 1f;
    protected float gunDamage;
    protected string gunDamageID;
    protected string fireRateID;
    protected string healthID;
    protected string bulletSpeedID;
    protected bool isTakingDamage = false;
    protected bool isReturningToPosition = false;
    protected float returnTimer = 0f;
    protected float returnDelay = 2f;
    protected float currentHealth;
    protected AudioManager audioManager;

    protected void Awake()
    {
        audioManager = AudioManager.Instance;

        if (audioManager == null)
            Debug.LogError("Audio Manager = null");
    }

    public virtual void Start()
    {
        probabilityManager = AbilityProbabilityManager.Instance;
        abilityManager = AbilityManager.Instance;
        stackBullets = StackBullets.Instance;

        gunDamageID = "Damage" + gunID;
        fireRateID = "FireRate" + gunID;
        healthID = "Health" + gunID;
        bulletSpeedID = "BulletSpeed" + gunID;

        gunDamage = PlayerPrefs.GetFloat(gunDamageID);
        fireRate = PlayerPrefs.GetFloat(fireRateID);
        maxHealth = PlayerPrefs.GetFloat(healthID);
        bulletSpeed = PlayerPrefs.GetFloat(bulletSpeedID);

        currentHealth = maxHealth;

        newMaterial = new Material(firingEffect.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader);

        if (GameManager.Instance.isTutorial)
        {
            gunDamage = GameManager.Instance.defaultGunSettings.damage;
            fireRate = GameManager.Instance.defaultGunSettings.fireRate;
            maxHealth = GameManager.Instance.defaultGunSettings.health;
            bulletSpeed = GameManager.Instance.defaultGunSettings.bulletSpeed;
        }
        else
            GetPasiveAbility();

        InvokeRepeating("UpdateTarget", 0f, 0.5f);
    }
    public virtual void UpdateTarget()
    {
        List<Enemy> enemies = Spawner.Instance.enemies;
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
            target = nearestEnemy.transform;
        else
            target = null;

    }

    public void Update()
    {
        if (bulletInAmmoGO == null)
        {
            if (!needBullet)
            {
                bulletInAmmoGO = Instantiate(bulletPrefab);
                bulletInAmmo = bulletInAmmoGO.GetComponent<Bullet>();

                bulletInAmmo.speed = bulletSpeed;

                bulletInAmmo.SetOnWaitingState();

            }
            else if (stackBullets.HaveBullet)
            {
                bulletInAmmoGO = stackBullets.bullets[stackBullets.bullets.Count - 1];
                bulletInAmmo = bulletInAmmoGO.GetComponent<Bullet>();

                bulletInAmmo.speed = bulletSpeed;

                bulletInAmmo.SetOnWaitingState();

                stackBullets.RemoveBulletFromStack();
            }
        }

        if (target == null || bulletInAmmoGO == null)
        {
            returnTimer += Time.deltaTime;
            if (returnTimer >= returnDelay && !isReturningToPosition)
            {
                StartCoroutine(SmoothReturnToPosition());
            }
            return;
        }
        else
        {
            returnTimer = 0f;
        }

        if (bulletInAmmoGO != null)
        {
            LockOnTarget();

            Vector3 forward = partToRotate.forward;
            forward.y = 0;
            Vector3 targetDirection = target.position - transform.position;
            targetDirection.y = 0;

            if (Vector3.Angle(forward, targetDirection) < 5f)
            {
                if (fireCountDown <= 0f)
                {
                    if (bulletInAmmoGO == null)
                        Debug.LogError("There is no bullet");

                    bulletInAmmoGO.SetActive(true);

                    Shoot(bulletInAmmoGO);
                    ShowParticles();

                    bulletInAmmoGO = null;

                    fireCountDown = 1f / fireRate;
                }
                else
                    fireCountDown -= Time.deltaTime;
            }
        }
    }

    public void ShowParticles()
    {
        bulletColor = bulletInAmmo.trailColor;
        GameObject effectIns = Instantiate(firingEffect, firePoint.position, Quaternion.identity);

        newMaterial.color = bulletColor;

        effectIns.GetComponent<ParticleSystemRenderer>().material = newMaterial;

        Destroy(effectIns, 3f);
    }

    public void LockOnTarget()
    {
        Vector3 dir = target.position - transform.position;
        Quaternion lookRotation = Quaternion.LookRotation(dir);
        Vector3 rotation = Quaternion.Lerp(partToRotate.rotation, lookRotation, Time.deltaTime * turnSpeed).eulerAngles;
        partToRotate.rotation = Quaternion.Euler(rotation.x, rotation.y, 0f);
    }

    private IEnumerator SmoothReturnToPosition()
    {
        isReturningToPosition = true;
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward);
        while (Quaternion.Angle(partToRotate.rotation, targetRotation) > 0.01f && (target == null || bulletInAmmoGO == null))
        {
            partToRotate.rotation = Quaternion.Lerp(partToRotate.rotation, targetRotation, Time.deltaTime * turnSpeed);
            yield return null;
        }
        isReturningToPosition = false;
    }


    public abstract void Shoot(GameObject bulletGO);

    public void TryGiveEffect(Bullet bullet)
    {
        if (pasiveAbility == PasiveAbility.FireBullet || abilityManager.selectedAbilities.Contains(AbilityManager.Ability.FireBullet))
        {
            if (Random.value < probabilityManager.GetProbability(AbilityManager.Ability.FireBullet))
            {
                bullet.effect = Bullet.Effect.Burn;
                bullet.trailColor = Color.red;
                bullet.UpdateTrailColor();
                return;
            }
        }
        if (pasiveAbility == PasiveAbility.FreezeBullet || abilityManager.selectedAbilities.Contains(AbilityManager.Ability.FreezeBullet))
        {
            if (Random.value < probabilityManager.GetProbability(AbilityManager.Ability.FreezeBullet))
            {
                bullet.effect = Bullet.Effect.Freeze;
                bullet.trailColor = Color.blue;
                bullet.UpdateTrailColor();
                return;
            }
        }
        if (pasiveAbility == PasiveAbility.DeadBullet || abilityManager.selectedAbilities.Contains(AbilityManager.Ability.DeadBullet))
        {
            if (Random.value < probabilityManager.GetProbability(AbilityManager.Ability.DeadBullet))
            {
                bullet.effect = Bullet.Effect.InstantDeath;
                bullet.trailColor = Color.black;
                bullet.UpdateTrailColor();
                return;
            }
        }
        if (pasiveAbility == PasiveAbility.PoisonBullet || abilityManager.selectedAbilities.Contains(AbilityManager.Ability.PoisonBullet))
        {
            if (Random.value < probabilityManager.GetProbability(AbilityManager.Ability.PoisonBullet))
            {
                bullet.effect = Bullet.Effect.Poison;
                bullet.trailColor = Color.green;
                bullet.UpdateTrailColor();
                return;
            }
        }
        if (pasiveAbility == PasiveAbility.StunBullet || abilityManager.selectedAbilities.Contains(AbilityManager.Ability.StunBullet))
        {
            if (Random.value < probabilityManager.GetProbability(AbilityManager.Ability.StunBullet))
            {
                bullet.effect = Bullet.Effect.Stun;
                bullet.trailColor = Color.white;
                bullet.UpdateTrailColor();
                return;
            }
        }
        if (pasiveAbility == PasiveAbility.DamageIncearedBullet || abilityManager.selectedAbilities.Contains(AbilityManager.Ability.DamageIncreasedBullet))
        {
            if (Random.value < probabilityManager.GetProbability(AbilityManager.Ability.DamageIncreasedBullet))
            {
                bullet.effect = Bullet.Effect.IncearedDamage;
                bullet.trailColor = Color.cyan;
                bullet.UpdateTrailColor();
                return;
            }
        }
        if (pasiveAbility == PasiveAbility.ExlosionBullet || abilityManager.selectedAbilities.Contains(AbilityManager.Ability.ExlosionBullet))
        {
            if (Random.value < probabilityManager.GetProbability(AbilityManager.Ability.ExlosionBullet))
            {
                bullet.effect = Bullet.Effect.Explosion;
                bullet.trailColor = Color.yellow;
                bullet.UpdateTrailColor();
                return;
            }
        }
    }

    public void GetPasiveAbility()
    {
        int abilityCount = System.Enum.GetValues(typeof(PasiveAbility)).Length;
        pasiveAbility = (PasiveAbility)Random.Range(1, abilityCount);
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        healthCanvas.SetActive(true);

        healthBar.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
        {
            GameManager.Instance.GunDestoyed(this);
            Destroy(gameObject);
        }
    }
}
