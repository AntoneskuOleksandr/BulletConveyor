public readonly struct WeaponProgressRO
{
    public int WeaponId { get; }
    public int DamageLevel { get; }
    public int FireRateLevel { get; }
    public int HealthLevel { get; }

    public WeaponProgressRO(int id, int dmg, int fr, int hp)
    {
        WeaponId = id;
        DamageLevel = dmg;
        FireRateLevel = fr;
        HealthLevel = hp;
    }
}

