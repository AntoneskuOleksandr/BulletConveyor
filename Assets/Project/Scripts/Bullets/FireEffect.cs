using System.Collections;
using UnityEngine;
using static EffectChanceManager;

public class FireEffect : MonoBehaviour, IProjectileEffect
{
    private float _duration = 3f;
    private float _DPS = 5f;
    public ProjectileEffectType EffectType => ProjectileEffectType.Fire;

    public void Apply(Bullet bullet, Enemy enemy)
    {
        enemy.StartCoroutine(BurnRoutine(enemy, _duration, _DPS)); // 3 секунды, 5 урона/сек
    }

    private IEnumerator BurnRoutine(Enemy enemy, float duration, float dps)
    {
        float elapsed = 0f;
        while (elapsed < duration && enemy.IsAlive)
        {
            enemy.TakeDamage(dps * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
