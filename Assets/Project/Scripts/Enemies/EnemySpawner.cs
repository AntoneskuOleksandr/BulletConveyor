using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField, Min(0f)] private float separation = 0.35f;
    [SerializeField, Min(0.05f)] private float spawnInterval = 0.2f;
    [Inject] private DiContainer _container;
    [Inject] private EnemyManager _enemyManager;
    [Inject] private WeaponManager _weaponManager;
    private GameObject enemyPrefab1;
    private GameObject enemyPrefab2;
    private readonly Collider[] overlaps = new Collider[32];
    private NavMeshPath path;
    private int totalSpawnedEnemies;
    private int obstacleMask;

    public void Initialize(GameObject type1, GameObject type2)
    {
        enemyPrefab1 = type1;
        enemyPrefab2 = type2 != null ? type2 : type1;
        obstacleMask = LayerMask.GetMask("Default", "Barrier", "Environment", "Weapon");
        Physics.SyncTransforms();
    }

    public IEnumerator SpawnWave(int count, int waveIndex, int totalWaves, Action<Enemy> onSpawned)
    {
        var delay = new WaitForSeconds(spawnInterval);
        float type2Chance = Mathf.Clamp01((waveIndex - totalWaves * 0.5f) / Mathf.Max(1f, totalWaves * 0.5f));
        for (int i = 0; i < count; i++)
        {
            var prefab = UnityEngine.Random.value < type2Chance ? enemyPrefab2 : enemyPrefab1;
            Vector3 position;
            while (!TryFindSpawnPosition(prefab, out position))
                yield return delay;
            var go = _container.InstantiatePrefab(prefab, position, Quaternion.Euler(0f, 180f, 0f), transform);
            go.name = $"Enemy({++totalSpawnedEnemies})";
            var enemy = go.GetComponent<Enemy>();
            _enemyManager.RegisterEnemy(enemy);
            onSpawned(enemy);
            yield return delay;
        }
    }

    public bool TryFindSpawnPosition(GameObject prefab, out Vector3 position)
    {
        path ??= new NavMeshPath();
        position = default;
        var agent = prefab.GetComponent<NavMeshAgent>();
        float radius = GetRadius(prefab);
        float height = Mathf.Max(radius * 2f, agent.height);
        var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
        var target = _weaponManager.GetRandomWeapon();
        if (target == null || !NavMesh.SamplePosition(target.transform.position, out var destination, 5f, filter))
            return false;
        for (int attempt = 0; attempt < 48; attempt++)
        {
            var candidate = new Vector3(
                UnityEngine.Random.Range(Mathf.Min(pointA.position.x, pointB.position.x), Mathf.Max(pointA.position.x, pointB.position.x)),
                pointA.position.y,
                UnityEngine.Random.Range(Mathf.Min(pointA.position.z, pointB.position.z), Mathf.Max(pointA.position.z, pointB.position.z)));
            if (!NavMesh.SamplePosition(candidate, out var hit, 1f, filter)) continue;
            if (hit.position.x < Mathf.Min(pointA.position.x, pointB.position.x) ||
                hit.position.x > Mathf.Max(pointA.position.x, pointB.position.x) ||
                hit.position.z < Mathf.Min(pointA.position.z, pointB.position.z) ||
                hit.position.z > Mathf.Max(pointA.position.z, pointB.position.z)) continue;
            if (!NavMesh.FindClosestEdge(hit.position, out var edge, filter) || edge.distance < radius) continue;
            if (!HasClearance(hit.position, radius, height)) continue;
            if (!NavMesh.CalculatePath(hit.position, destination.position, filter, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            position = hit.position;
            return true;
        }
        return false;
    }

    public bool HasClearance(Vector3 position, float radius, float height)
    {
        foreach (var enemy in _enemyManager.Enemies)
        {
            if (enemy == null || !enemy.IsAlive) continue;
            var delta = enemy.transform.position - position;
            delta.y = 0f;
            float distance = radius + enemy.BodyRadius + separation;
            if (delta.sqrMagnitude < distance * distance) return false;
        }
        var bottom = position + Vector3.up * (radius + 0.25f);
        var top = position + Vector3.up * Mathf.Max(radius + 0.25f, height - radius);
        return Physics.OverlapCapsuleNonAlloc(bottom, top, radius + 0.1f, overlaps, obstacleMask,
            QueryTriggerInteraction.Ignore) == 0;
    }

    public static float GetRadius(GameObject prefab)
    {
        float radius = prefab.GetComponent<NavMeshAgent>().radius;
        if (prefab.TryGetComponent<CapsuleCollider>(out var capsule))
            radius = Mathf.Max(radius, capsule.radius * Mathf.Max(prefab.transform.lossyScale.x, prefab.transform.lossyScale.z));
        return radius;
    }
}
