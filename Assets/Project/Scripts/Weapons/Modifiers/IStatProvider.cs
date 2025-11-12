public interface IStatProvider
{
    float ModifyFireRate(float baseValue) => baseValue;
    float ModifyDamage(float baseValue) => baseValue;
    float ModifyHealth(float baseValue) => baseValue;
    float ModifyAttackRadius(float baseValue) => baseValue;
}