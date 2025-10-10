using UnityEngine;
using Zenject;

public enum ShootStrategyType { Basic, Burst }
public enum AimStrategyType { Basic, Predictive }
public enum TargetStrategyType { Nearest, Heaviest }

public interface IShootStrategy
{
    void Shoot(Weapon weapon);
}

public interface IAimStrategy
{
    void Aim(Weapon weapon);
}

public interface ITargetStrategy
{
    Enemy FindTarget(Weapon weapon);
}

public class NearestTargetStrategy : ITargetStrategy
{
    [Inject] private EnemyManager _enemyManager;

    public Enemy FindTarget(Weapon weapon)
    {
        var currentTarget = _enemyManager.GetNearestEnemy(weapon, weapon.AttackRadius);
        return currentTarget;
    }
}

public class BasicAimStrategy : IAimStrategy
{
    public void Aim(Weapon weapon)
    {
        var target = weapon.Target;
        if (target == null || weapon.PartToRotate == null)
            return;

        Vector3 dir = target.HitTransform.position - weapon.PartToRotate.position;
        dir.y = 0f;
        if (dir == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(dir);
        weapon.PartToRotate.rotation = Quaternion.Lerp(
            weapon.PartToRotate.rotation,
            lookRotation,
            Time.deltaTime * weapon.TurnSpeed
        );
    }
}

public class BasicShootStrategy : IShootStrategy
{
    [Inject] private BulletManager _bulletManager;

    public void Shoot(Weapon weapon)
    {
        // Проверяем таймер
        if (weapon.FireCountdown > 0f)
            return;

        var target = weapon.Target;
        if (target == null) return;

        // Проверяем прицеливание
        if (!IsAimedAtTarget(weapon, target))
            return;

        // Берем пулю и стреляем
        var bullet = _bulletManager.TryGetBullet(weapon);
        if (bullet == null) return;

        bullet.Init(weapon.Damage, weapon.FirePoint, target.HitTransform);

        weapon.ResetFireCountdown();
    }

    private bool IsAimedAtTarget(Weapon weapon, Enemy target)
    {
        Vector3 dirToTarget = target.HitTransform.position - weapon.PartToRotate.position;
        dirToTarget.y = 0f;
        dirToTarget.Normalize();

        Vector3 forward = weapon.PartToRotate.forward;
        forward.y = 0f;
        forward.Normalize();

        float angle = Vector3.Angle(forward, dirToTarget);
        return angle < weapon.AimThreshold;
    }
}
