using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class BeamInstantMoveStrategy : IProjectileMoveStrategy
{
    public void Launch(Weapon weapon, ProjectileBase projectile, ProjectileInitData data)
    {
        float _range = weapon.Data.GetParameter<float>(WeaponParameterType.Length);
        int _pierceCount = weapon.Data.GetParameter<int>(WeaponParameterType.PierceCount);
        float _visualLifetime = weapon.Data.GetParameter<float>(WeaponParameterType.LifeTime);
        float _beamWidth = weapon.Data.GetParameter<float>(WeaponParameterType.BeamWidth);
        Color _beamColor = weapon.Data.GetParameter<Color>(WeaponParameterType.Color);

        projectile.gameObject.SetActive(false);

        Vector3 origin = projectile.transform.position;
        Vector3 direction = data.HasOverrideForward ? data.OverrideForward.normalized : projectile.transform.forward;

        var hits = Physics.RaycastAll(origin, direction, _range);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        int remainingPierce = _pierceCount;
        Vector3? lastEffectiveHit = null;

        foreach (var hit in hits)
        {
            var enemy = hit.collider.GetComponent<Enemy>();
            if (enemy == null)
                continue;

            if (remainingPierce <= 0)
                break;

            enemy.TakeDamage(data.Damage);
            projectile.ApplyEffects(enemy);
            remainingPierce--;
            lastEffectiveHit = hit.point;
        }

        Vector3 endPoint = (remainingPierce > 0)
            ? origin + direction * _range
            : (lastEffectiveHit ?? origin + direction * _range);

        GameObject beamGO = new GameObject("BeamVisual");
        BeamVisual visual = beamGO.AddComponent<BeamVisual>();

        float distance = Vector3.Distance(origin, endPoint);
        visual.Configure(_beamColor, _beamWidth, distance, _visualLifetime,
            weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint));

        if (projectile.Collider != null)
            projectile.Collider.enabled = false;
        if (projectile.Rigidbody != null)
            projectile.Rigidbody.isKinematic = true;

        Object.Destroy(projectile.gameObject, _visualLifetime);
    }

    public void Update(Weapon weapon, ProjectileBase projectile, float deltaTime) { }
}
