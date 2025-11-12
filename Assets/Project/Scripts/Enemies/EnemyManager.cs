using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class EnemyManager : MonoBehaviour
{
    public event Action<Enemy> OnEnemyDied;

    public List<Enemy> Enemies { get; private set; }

    [Inject] private PlayerRuntimeManager _playerRuntimeManager;

    private void Awake()
    {
        Enemies = new List<Enemy>();
    }

    public void RegisterEnemies(List<Enemy> enemies)
    {
        Enemies.AddRange(enemies);

        foreach (Enemy enemy in enemies)
        {
            enemy.OnDeath.AddListener(() => UnregisterEnemy(enemy));
        }
    }

    private void UnregisterEnemy(Enemy enemy)
    {
        if (Enemies.Contains(enemy))
        {
            _playerRuntimeManager.AddCurrency(CurrencyType.Coins, enemy.CoinsKillReward);
            _playerRuntimeManager.AddCurrency(CurrencyType.Gears, enemy.GearsKillReward);
            Enemies.Remove(enemy);
            OnEnemyDied?.Invoke(enemy);
        }
    }
}
