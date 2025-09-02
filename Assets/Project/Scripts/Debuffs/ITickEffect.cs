public interface ITickEffect : IStatusEffect
{
    float DPS { get; }
    void Tick(float deltaTime, Enemy enemy);
    bool IsFinished();
}
