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

        return _enemyManager.GetNearestEnemy(weapon, weapon.FindTargetRadius) ?? null;
    }
}
