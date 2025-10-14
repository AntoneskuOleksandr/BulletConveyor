public interface IShootStrategy
{
    /// <summary>
    /// Возвращает true, если выстрел реально произошёл.
    /// </summary>
    bool TryShoot(Weapon weapon);
}
