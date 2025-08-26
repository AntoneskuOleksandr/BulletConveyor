using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> _enemies = new List<Enemy>();

    public void RegisterEnemies(List<Enemy> enemies)
    {
        _enemies.AddRange(enemies);
    }

    public Transform GetNearestEnemy(Weapon weapon)
    {
        //Debug.Log($"Finding nearest enemy for weapon {weapon}");
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in _enemies)
        {
            float distanceToEnemy = Vector3.Distance(weapon.transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        //Debug.Log($"Nearest enemy found: {nearestEnemy} at distance {shortestDistance}");

        return nearestEnemy.transform;
    }
}
