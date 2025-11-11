using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

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

        // создаём визуал дочерним FirePoint
        GameObject beamGO = new GameObject("Beam_Continuous");
        beamGO.transform.SetParent(firePoint, worldPositionStays: false);
        beamGO.transform.localPosition = Vector3.zero;
        beamGO.transform.localRotation = Quaternion.identity;

        var visual = beamGO.AddComponent<BeamVisual>();
        visual.Configure(beamColor, beamWidth, range, lifeTime);

        // запускаем корутину урона
        weapon.StartCoroutine(DamageRoutine(weapon, projectile, data, firePoint, range, pierceCount, tickInterval, lifeTime));
    }

    public void Update(Weapon weapon, ProjectileBase projectile, float deltaTime) { }

    private IEnumerator DamageRoutine(Weapon weapon, ProjectileBase projectile, ProjectileInitData data,
                                      Transform firePoint, float range, int pierceCount,
                                      float tickInterval, float lifeTime)
    {
        float elapsed = 0f;

        while (elapsed < lifeTime)
        {
            Vector3 origin = firePoint.position;
            Vector3 direction = firePoint.forward;

            var hits = Physics.RaycastAll(origin, direction, range);
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            int remainingPierce = pierceCount;

            foreach (var hit in hits)
            {
                if (remainingPierce <= 0)
                    break;

                var enemy = hit.collider.GetComponent<Enemy>();
                if (enemy == null)
                    continue;

                enemy.TakeDamage(data.Damage);
                projectile?.ApplyEffects(enemy);
                remainingPierce--;
            }

            yield return new WaitForSeconds(tickInterval);
            elapsed += tickInterval;
        }

        // визуал сам удалится по таймеру
    }
}
