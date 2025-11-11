using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
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
    private ObjectPool<GameObject> projectilePool;

    [Inject]
    public void Construct(BoardManager boardManager)
    {
        _boardManager = boardManager;
    }

    public void Initialize()
    {
        projectilesFolder = new GameObject("Projectiles");

        projectilePool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(projectilePrefab),
            actionOnGet: projectile =>
            {
                projectile.SetActive(true);
            },
            actionOnRelease: projectile =>
            {
                projectile.SetActive(false);
            },
            actionOnDestroy: projectile => Destroy(projectile),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 100
        );

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

            index = 0;

            blocksPool.transform.DOMoveY(-animationStrength, pulseAnimationDuration).SetLoops(2, LoopType.Yoyo);

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

        GameObject projectile = projectilePool.Get();
        projectile.transform.position = spawnPosition;
        projectile.transform.Rotate(0f, -90f, 0f);
        projectile.transform.parent = projectilesFolder.transform;

        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        projectile.transform.DOMove(conveyor.position + new Vector3(0, 0f, index * spawnOffset), projectileAnimationDuration).OnComplete(() =>
        {
            rb.freezeRotation = false;
            rb.isKinematic = true;
        });

        index++;
    }
}
