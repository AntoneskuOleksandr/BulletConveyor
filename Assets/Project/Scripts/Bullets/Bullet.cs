using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    public float speed = 20f;
    public float damage = 10f;

    protected Transform _target;
    protected Vector3 _direction;
    protected Rigidbody _rigidbody;
    protected Collider _collider;

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

        this.damage = damage;
        this._target = target;

        this.transform.position = spawnPoint.position;
        this.transform.rotation = spawnPoint.rotation;

        Launch();
    }

    private void Launch()
    {
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = false;
        _collider.isTrigger = true;

        _direction = _target.position - this.transform.position;
        _rigidbody.linearVelocity = _direction.normalized * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Bullet hit: " + other.gameObject.name);

        if (other.GetComponent<Weapon>() != null)
        {
            return; // Ignore collisions with weapons
        }

        Destroy(this.gameObject);
    }
}
