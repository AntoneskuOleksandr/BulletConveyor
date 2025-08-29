using System.Collections.Generic;
using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    protected Transform _target;
    protected Vector3 _direction;
    protected Rigidbody _rigidbody;
    protected Collider _collider;

    protected float _speed = 20f;
    protected float _damage = 10f;

    protected List<IOnHitEffect> _effects = new List<IOnHitEffect>();

    public float Damage => _damage;
    public float Speed => _speed;

    public void AddEffect(IOnHitEffect effect) => _effects.Add(effect);

    public virtual void Init(float damage, Transform spawnPoint, Transform target)
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();

        if (_rigidbody == null)
        {
            Debug.LogError("Rigidbody component is missing on the bullet.");
            return;
        }
        if (_collider == null)
        {
            Debug.LogError("Collider component is missing on the bullet.");
            return;
        }

        this._damage = damage;
        this._target = target;

        this.transform.position = spawnPoint.position;
        this.transform.rotation = spawnPoint.rotation;

        Launch();
    }

    protected void Launch()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = false;
        _collider.enabled = true;
        _collider.isTrigger = true;

        _direction = _target.position - this.transform.position;
        _rigidbody.linearVelocity = _direction.normalized * Speed;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Enemy"))
        {
            HitTarget(other.transform);
        }

        Destroy(this.gameObject);
    }

    protected void HitTarget(Transform target)
    {
        Enemy enemy = target.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(Damage); //Base damage
            foreach (var effect in _effects)
            {
                effect.Apply(this, enemy);
            }
        }
    }
}
