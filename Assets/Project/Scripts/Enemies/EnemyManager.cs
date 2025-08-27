using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> _enemies = new List<Enemy>();

    public void RegisterEnemies(List<Enemy> enemies)
    {
        _enemies.AddRange(enemies);
    }

    public Transform GetNearestEnemy(Weapon weapon, float radius)
    {
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in _enemies)
        {
            if (enemy == null)
                continue;

            float distanceToEnemy = Vector3.Distance(weapon.transform.position, enemy.HitTransform.position);

            // игнорируем врагов вне радиуса действия
            if (distanceToEnemy > radius)
                continue;

            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        return nearestEnemy != null ? nearestEnemy.HitTransform : null;
    }
}
