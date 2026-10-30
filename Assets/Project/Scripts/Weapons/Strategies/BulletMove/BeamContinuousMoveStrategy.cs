using System;
using System.Collections;
using UnityEngine;

public class BeamContinuousMoveStrategy : IProjectileMoveStrategy
{
    public void Launch(Weapon weapon, ProjectileBase projectile, ProjectileInitData data)
    {
        float range = weapon.Data.GetParameter<float>(WeaponParameterType.Length);
        int pierceCount = weapon.Data.GetParameter<int>(WeaponParameterType.PierceCount);
        float tickInterval = weapon.Data.GetParameter<float>(WeaponParameterType.TickInterval);
        float lifeTime = weapon.Data.GetParameter<float>(WeaponParameterType.LifeTime);
        float beamWidth = weapon.Data.GetParameter<float>(WeaponParameterType.BeamWidth);
        Color beamColor = weapon.Data.GetParameter<Color>(WeaponParameterType.Color);

        projectile.gameObject.SetActive(false);

        Transform firePoint = weapon.AttachmentPoints.Get(WeaponAttachmentType.FirePoint);

        GameObject beamGO = new GameObject("Beam_Continuous");
        beamGO.transform.SetParent(firePoint, worldPositionStays: false);

        if (data.HasOverrideForward)
            beamGO.transform.rotation = Quaternion.LookRotation(data.OverrideForward.normalized);
        else
            beamGO.transform.rotation = firePoint.rotation;

        BeamVisual visual = beamGO.AddComponent<BeamVisual>();
        visual.Configure(beamColor, beamWidth, range, lifeTime, firePoint);

        weapon.StartCoroutine(DamageRoutine(
            weapon, projectile, data,
            firePoint, range, pierceCount, tickInterval, lifeTime, visual));
    }

    public void Update(Weapon weapon, ProjectileBase projectile, float deltaTime) { }

    private IEnumerator DamageRoutine(
    Weapon weapon, ProjectileBase projectile, ProjectileInitData data,
    Transform firePoint, float range, int pierceCount,
    float tickInterval, float lifeTime, BeamVisual visual)
    {
        float elapsed = 0f;

        while (elapsed < lifeTime)
        {
            Vector3 origin = firePoint.position;
            Vector3 direction;

            if (data.HasOverrideForward)
            {
                direction = (visual.transform.forward).normalized;
            }
            else
            {
                direction = firePoint.forward;
            }

            var hits = Physics.RaycastAll(origin, direction, range);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            int remainingPierce = pierceCount;
            Vector3? lastHitPoint = null;

            foreach (var hit in hits)
            {
                if (remainingPierce <= 0)
                    break;

                var enemy = hit.collider.GetComponent<Enemy>();
                if (enemy == null)
                    continue;

                enemy.TakeDamage(data.Damage);
                projectile.ApplyEffects(enemy);

                remainingPierce--;
                lastHitPoint = hit.point;
            }

            Vector3 endPoint = (remainingPierce > 0)
                ? origin + direction * range
                : (lastHitPoint ?? origin + direction * range);

            float currentLength = Vector3.Distance(origin, endPoint);
            visual.UpdateLength(currentLength);

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }
    }
}
