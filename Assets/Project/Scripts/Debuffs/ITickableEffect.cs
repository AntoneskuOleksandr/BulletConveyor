public interface ITickableEffect : IStatusEffect
{
    void Tick(float deltaTime, Enemy enemy);
}