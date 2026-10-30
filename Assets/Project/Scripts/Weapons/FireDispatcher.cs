using System.Collections.Generic;
using Zenject;

public class FireDispatcher : ITickable
{
    private readonly List<FireRequest> _pending = new();
    private readonly List<FireRequest> _buffer = new();

    [Inject] private ProjectileManager _projectileManager;

    public void Enqueue(FireRequest request)
    {
        _pending.Add(request);
    }

    public void Tick()
    {
        if (_pending.Count == 0)
            return;

        _buffer.Clear();
        _buffer.AddRange(_pending);
        _pending.Clear();

        _buffer.Sort((a, b) =>
        {
            int prio = b.Priority.CompareTo(a.Priority);
            if (prio != 0) return prio;
            return a.SequenceId.CompareTo(b.SequenceId);
        });

        for (int i = 0; i < _buffer.Count; i++)
        {
            var r = _buffer[i];
            if (r.Weapon == null || !r.Weapon.IsAlive) continue;
            if (r.Weapon.Target == null || !r.Weapon.Target.IsAlive)
            {
                r.Weapon.WaitingForFire = false;
                continue;
            }
            var allocation = _projectileManager.Allocate(r.Min, r.Max);

            if (!allocation.Success || allocation.Projectiles == null || allocation.Projectiles.Count == 0)
            {
                _pending.Add(r);
                continue;
            }

            r.ShootStrategy.ShootAllocated(r.Weapon, allocation.Projectiles);
        }
    }
}

public class FireRequest
{
    private static long _counter = 0;

    public Weapon Weapon { get; }
    public IShootStrategy ShootStrategy { get; }
    public int Min { get; }
    public int Max { get; }
    public float Priority { get; }
    public long SequenceId { get; }

    public FireRequest(Weapon weapon, IShootStrategy shootStrategy, int min, int max, float priority)
    {
        Weapon = weapon;
        ShootStrategy = shootStrategy;
        Min = min;
        Max = max;
        Priority = priority;
        SequenceId = _counter++;
    }
}
