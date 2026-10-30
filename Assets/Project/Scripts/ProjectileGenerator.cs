using DG.Tweening;
using System.Collections;
using UnityEngine;
using Zenject;

public class ProjectileGenerator : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnOffset = 1f;
    [SerializeField] private Transform conveyor;
    [SerializeField] private GameObject projectilePrefab;

    [Header("Animations")]
    [SerializeField] private float projectileAnimationDuration = 1f;
    [SerializeField] private float pulseAnimationDuration = 0.2f;
    [SerializeField] private float animationStrength = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = false;
    [SerializeField] private int debugProjectilesCount = 3;
    [SerializeField] private bool spawnOnceOnStart = false;

    private BoardManager _boardManager;
    public GameObject blocksPool;

    private int index = 0;

    private GameObject projectilesFolder;

    [Inject]
    public void Construct(BoardManager boardManager)
    {
        _boardManager = boardManager;
    }

    public void Initialize()
    {
        projectilesFolder = new GameObject("Projectiles");

        if (spawnOnceOnStart)
        {
            for (int i = 0; i < debugProjectilesCount; i++)
                GenerateProjectile(null);
        }
    }

    public IEnumerator StartProjectileGeneration()
    {
        if (spawnOnceOnStart)
            yield break;

        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (projectilesFolder.transform.childCount >= 150) continue;
            index = 0;

            blocksPool.transform.DOMoveY(-animationStrength, pulseAnimationDuration).SetLoops(2, LoopType.Yoyo).SetLink(blocksPool);

            if (debugMode)
            {
                for (int i = 0; i < debugProjectilesCount; i++)
                {
                    GenerateProjectile(null);
                }
            }
            foreach (BlockParent block in _boardManager.Blocks)
            {
                for (int i = 0; i < block.projectilesToGenerate; i++)
                {
                    if (projectilesFolder.transform.childCount >= 150) break;
                    GenerateProjectile(block);
                }
            }
        }
    }

    private void GenerateProjectile(BlockParent block)
    {
        Vector3 spawnPosition = this.transform.position;
        if (block != null)
            spawnPosition = block.transform.position;

        GameObject projectile = Instantiate(projectilePrefab, projectilesFolder.transform);
        projectile.transform.position = spawnPosition;
        projectile.transform.Rotate(0f, -90f, 0f);
        projectile.transform.parent = projectilesFolder.transform;

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        projectile.transform.DOMove(conveyor.position + new Vector3(0, 0f, index * spawnOffset), projectileAnimationDuration).SetLink(projectile).OnComplete(() =>
        {
            rb.freezeRotation = false;
            rb.isKinematic = true;
        });

        index++;
    }
}
