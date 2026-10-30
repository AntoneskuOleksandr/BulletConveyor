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
            if (enemy == null || !enemy.IsAlive)
                continue;

            float distanceToEnemy = (weapon.transform.position - enemy.HitTransform.position).sqrMagnitude;

            if (distanceToEnemy < weapon.MinAttackRadius * weapon.MinAttackRadius ||
                distanceToEnemy > weapon.MaxAttackRadius * weapon.MaxAttackRadius)
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
