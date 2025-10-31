using System.Collections.Generic;

public interface IShootStrategy
{
    /// <summary>
    /// Пытается произвести выстрел.
    /// Возвращает true, если выстрел состоялся.
    /// Все созданные пули добавляются в список spawnedBullets.
    /// </summary>
    bool IsCanShoot(Weapon weapon);
    List<Bullet> Shoot(Weapon weapon);
}
