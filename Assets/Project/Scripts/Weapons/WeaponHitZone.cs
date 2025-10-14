using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitZone : MonoBehaviour
{
    [SerializeField] private bool killEnemyOnContact = true;
    private Weapon _weapon;

    public void Initialize(Weapon weapon)
    {
        _weapon = weapon;
        Collider collider = GetComponent<Collider>();
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        _weapon?.TakeDamage(enemy.ContactDamage);

        if (killEnemyOnContact)
        {
            enemy.TakeDamage(enemy.Health);
        }
    }
}
