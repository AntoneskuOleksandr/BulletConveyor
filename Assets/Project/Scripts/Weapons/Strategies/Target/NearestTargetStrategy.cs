using UnityEngine;
using Zenject;

public class NearestTargetStrategy : ITargetStrategy
{
    [Inject] private EnemyManager _enemyManager;

    public Enemy FindTarget(Weapon weapon)
    {
        if (_enemyManager == null || weapon == null)
        {
            Debug.LogWarning("EnemyManager or Weapon is null in NearestTargetStrategy.");
            return null;
        }

        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in _enemyManager.Enemies)
        {
            if (enemy == null)
                continue;

            float distanceToEnemy = Vector3.Distance(weapon.transform.position, enemy.HitTransform.position);

            // игнорируем врагов вне радиуса действия
            if (distanceToEnemy < weapon.MinAttackRadius || distanceToEnemy > weapon.MaxAttackRadius)
                continue;

            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy;
    }
}
