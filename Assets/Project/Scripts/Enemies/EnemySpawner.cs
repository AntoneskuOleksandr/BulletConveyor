using System;
using System.Collections;
using System.Collections.Generic;
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
    [Inject] private LevelLoader _levelLoader;
    private GameObject enemyPrefab1;
    private GameObject enemyPrefab2;
    private readonly Collider[] overlaps = new Collider[32];
    private readonly List<Vector3> destinations = new(3);
    private NavMeshPath path;
    private int totalSpawnedEnemies;
    private int obstacleMask;
    private bool training;

    public void Initialize(GameObject type1, GameObject type2, bool isTraining = false)
    {
        enemyPrefab1 = type1;
        enemyPrefab2 = type2 != null ? type2 : type1;
        training = isTraining;
        obstacleMask = LayerMask.GetMask("Default", "Barrier", "Environment", "Weapon");
        Physics.SyncTransforms();
    }

    public static float GetSecondTypeChance(int waveIndex, int totalWaves)
    {
        int half = totalWaves / 2;
        return Mathf.Clamp01((float)(waveIndex - half) / Mathf.Max(1, totalWaves - half));
    }

    public IEnumerator SpawnWave(int count, int waveIndex, int totalWaves, Action<Enemy> onSpawned)
    {
        var delay = new WaitForSeconds(spawnInterval);
        float chance = GetSecondTypeChance(waveIndex, totalWaves);
        bool recovered = false;
        for (int i = 0; i < count; i++)
        {
            var prefab = chance >= 1f || UnityEngine.Random.value < chance ? enemyPrefab2 : enemyPrefab1;
            Vector3 position;
            int attempts = 0;
            while (!TryFindSpawnPosition(prefab, out position))
            {
                attempts++;
                if (attempts >= 5 && !recovered && _enemyManager.Enemies.Count == 0)
                {
                    recovered = true;
                    _levelLoader.RebuildNavigation();
                }
                if (attempts == 25) Debug.LogWarning($"Spawn waiting for clear navigation: {prefab.name}, wave {waveIndex}");
                yield return delay;
            }
            var go = _container.InstantiatePrefab(prefab, position, Quaternion.Euler(0f, 180f, 0f), transform);
            go.name = $"{prefab.name} ({++totalSpawnedEnemies})";
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
        float height = GetHeight(prefab, radius);
        var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
        destinations.Clear();
        foreach (var weapon in _weaponManager.ActiveWeapons)
            if (weapon != null && weapon.IsAlive && NavMesh.SamplePosition(weapon.transform.position, out var hit, 5f, filter))
                destinations.Add(hit.position);
        if (destinations.Count == 0) return false;
        GetBounds(out var min, out var max);
        for (int attempt = 0; attempt < 48; attempt++)
        {
            var candidate = new Vector3(UnityEngine.Random.Range(min.x, max.x), pointA.position.y,
                UnityEngine.Random.Range(min.z, max.z));
            if (TryCandidate(candidate, min, max, radius, height, filter, out position)) return true;
        }
        int columns = Mathf.Clamp(Mathf.FloorToInt((max.x - min.x) / (radius * 2f + separation)), 2, 12);
        int rows = Mathf.Clamp(Mathf.FloorToInt((max.z - min.z) / (radius * 2f + separation)), 2, 8);
        for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                var candidate = new Vector3(Mathf.Lerp(min.x, max.x, (column + 0.5f) / columns), pointA.position.y,
                    Mathf.Lerp(min.z, max.z, (row + 0.5f) / rows));
                if (TryCandidate(candidate, min, max, radius, height, filter, out position)) return true;
            }
        return false;
    }

    private void GetBounds(out Vector3 min, out Vector3 max)
    {
        min = Vector3.Min(pointA.position, pointB.position);
        max = Vector3.Max(pointA.position, pointB.position);
        if (!training) return;
        float center = (min.x + max.x) * 0.5f;
        float width = Mathf.Min(12f, max.x - min.x);
        min.x = center - width * 0.5f;
        max.x = center + width * 0.5f;
        max.z = Mathf.Min(max.z, min.z + 4f);
    }

    private bool TryCandidate(Vector3 candidate, Vector3 min, Vector3 max, float radius, float height,
        NavMeshQueryFilter filter, out Vector3 position)
    {
        position = default;
        if (!NavMesh.SamplePosition(candidate, out var hit, 1f, filter)) return false;
        if (hit.position.x < min.x || hit.position.x > max.x || hit.position.z < min.z || hit.position.z > max.z) return false;
        if (!NavMesh.FindClosestEdge(hit.position, out var edge, filter) || edge.distance < radius) return false;
        if (!HasClearance(hit.position, radius, height)) return false;
        foreach (var destination in destinations)
            if (NavMesh.CalculatePath(hit.position, destination, filter, path) && path.status == NavMeshPathStatus.PathComplete)
            {
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

    private static float GetHeight(GameObject prefab, float radius)
    {
        float height = prefab.GetComponent<NavMeshAgent>().height * Mathf.Abs(prefab.transform.lossyScale.y);
        if (prefab.TryGetComponent<CapsuleCollider>(out var capsule))
            height = Mathf.Max(height, capsule.height * Mathf.Abs(prefab.transform.lossyScale.y));
        return Mathf.Max(radius * 2f, height);
    }
}
