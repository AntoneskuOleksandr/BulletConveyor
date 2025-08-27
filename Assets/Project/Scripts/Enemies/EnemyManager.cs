using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> _enemies = new List<Enemy>();

    public event Action<Enemy> OnEnemyDied;

    public void RegisterEnemies(List<Enemy> enemies)
    {
        _enemies.AddRange(enemies);

        foreach (Enemy enemy in enemies)
        {
            enemy.OnDeath.AddListener(() => UnregisterEnemy(enemy));
        }
    }

    private void UnregisterEnemy(Enemy enemy)
    {
        if (_enemies.Contains(enemy))
        {
            OnEnemyDied?.Invoke(enemy);
            _enemies.Remove(enemy);
        }
    }

    public Enemy GetNearestEnemy(Weapon weapon, float radius)
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

        return nearestEnemy;
    }
}
