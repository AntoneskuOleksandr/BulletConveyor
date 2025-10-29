using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemyManager : MonoBehaviour
{
    public event Action<Enemy> OnEnemyDied;

    private List<Enemy> _enemies = new List<Enemy>();

    [Inject] private PlayerRuntimeManager _playerRuntimeManager;

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
            _playerRuntimeManager.AddCurrency(CurrencyType.Coins, enemy.CoinsKillReward);
            _playerRuntimeManager.AddCurrency(CurrencyType.Gears, enemy.GearsKillReward);
            _enemies.Remove(enemy);
            OnEnemyDied?.Invoke(enemy);
        }
    }

    public Enemy GetNearestEnemy(Weapon weapon, float minRadius, float maxRadius)
    {
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in _enemies)
        {
            if (enemy == null)
                continue;

            float distanceToEnemy = Vector3.Distance(weapon.transform.position, enemy.HitTransform.position);

            // игнорируем врагов вне радиуса действия
            if (distanceToEnemy < minRadius || distanceToEnemy > maxRadius)
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
