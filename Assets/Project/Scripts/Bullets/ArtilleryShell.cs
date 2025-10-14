using UnityEngine;

public class ArtilleryShell : Bullet
{
    public override void OnTriggerEnter(Collider other)
    {
        if (this.GetComponent<ArtilleryShell>().enabled)
            Explode();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
