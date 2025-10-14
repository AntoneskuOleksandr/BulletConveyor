public interface IAimStrategy
{
    /// <summary>
    /// ѕытаетс€ навестись на цель. 
    /// ¬озвращает true, если пушка в пределах допустимого угла и прицелилась.
    /// </summary>
    bool TryAim(Weapon weapon);
}
