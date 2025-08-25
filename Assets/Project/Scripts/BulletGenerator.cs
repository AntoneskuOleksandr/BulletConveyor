using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.Pool;
using Zenject;

public class BulletGenerator : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnOffset = 1f;
    [SerializeField] private Transform conveyor;
    [SerializeField] private GameObject bulletPrefab;

    [Header("Animations")]
    [SerializeField] private float bulletAnimationDuration = 1f;
    [SerializeField] private float pulseAnimationDuration = 0.2f;
    [SerializeField] private float animationStrength = 0.2f;

    private BoardManager _boardManager;
    public GameObject blocksPool;

    private int index = 0;

    private GameObject bulletsFolder;
    private ObjectPool<GameObject> bulletPool;

    [Inject]
    public void Construct(BoardManager boardManager)
    {
        _boardManager = boardManager;
    }

    public void Initialize()
    {
        bulletsFolder = new GameObject("Bullets");

        bulletPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(bulletPrefab),
            actionOnGet: bullet =>
            {
                bullet.SetActive(true);
            },
            actionOnRelease: bullet =>
            {
                bullet.SetActive(false);
            },
            actionOnDestroy: bullet => Destroy(bullet),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 100
        );
    }

    public IEnumerator StartBulletGeneration()
    {
        Debug.Log("Bullet Generation Started");
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            index = 0;

            blocksPool.transform.DOMoveY(-animationStrength, pulseAnimationDuration).SetLoops(2, LoopType.Yoyo);

            foreach (BlockParent block in _boardManager.Blocks)
            {
                for (int i = 0; i < block.bulletsToGenerate; i++)
                {
                    GenerateBullet(block);
                }
            }
        }
    }

    private void GenerateBullet(BlockParent block)
    {
        Vector3 spawnPosition = block.transform.position;
        GameObject bullet = bulletPool.Get();
        bullet.transform.position = spawnPosition;
        bullet.transform.Rotate(0f, -90f, 0f);
        bullet.transform.parent = bulletsFolder.transform;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        bullet.transform.DOMove(conveyor.position + new Vector3(0, 0f, index * spawnOffset), bulletAnimationDuration).OnComplete(() =>
        {
            rb.freezeRotation = false;
            rb.isKinematic = true;
        });

        index++;
    }
}
