using UnityEngine;
using Zenject;

public class NearestTargetStrategy : ITargetStrategy
{
    [Inject] private EnemyManager _enemyManager;

    public Enemy FindTarget(Weapon weapon)
    {
        if (_enemyManager == null || _enemyManager.Enemies == null || _enemyManager.Enemies.Count == 0 || weapon == null)
            return null;

        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in _enemyManager.Enemies)
        {
            if (enemy == null)
                continue;

            float distanceToEnemy = Vector3.Distance(weapon.transform.position, enemy.HitTransform.position);

            if (distanceToEnemy < weapon.MinAttackRadius || distanceToEnemy > weapon.FindTargetRadius)
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
