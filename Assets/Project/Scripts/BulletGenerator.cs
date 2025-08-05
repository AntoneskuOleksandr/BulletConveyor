using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using DG.Tweening;
using System.Collections;

public class BulletGenerator : MonoBehaviour
{
    [SerializeField] private float spawnInterval = 1f;
    [SerializeField] private float spawnOffset = 1f;
    [SerializeField] private Transform conveyor;

    [Header("Animations")]
    [SerializeField] private float bulletAnimationDuration = 0.2f;
    [SerializeField] private float pulseAnimationDuration = 0.2f;
    [SerializeField] private float animationStrength = 0.2f;

    public GameObject blocksPool;
    public List<BlockParent> blockParents;

    private float timer;
    private int index = 0;

    private GameObject bulletsFolder;
    private ObjectPool<GameObject> bulletPool;

    private void Start()
    {
        timer = spawnInterval - 1;
        bulletsFolder = new GameObject("Bullets");

        bulletPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(blockParents[0].bulletPrefab),
            actionOnGet: bullet =>
            {
                bullet.GetComponent<Bullet>().enabled = false;
                bullet.GetComponent<Bullet>().OnBulletHit += ReleaseBullet;
                bullet.GetComponent<Bullet>().trail.enabled = false;
                bullet.SetActive(true);
            },
            actionOnRelease: bullet =>
            {
                bullet.SetActive(false);
                bullet.GetComponent<Bullet>().OnBulletHit -= ReleaseBullet;
            },
            actionOnDestroy: bullet => Destroy(bullet),
            collectionCheck: false,
            defaultCapacity: 10,
            maxSize: 100
        );

        StartCoroutine(SpawnBullets());
    }

    private IEnumerator SpawnBullets()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (blockParents.Count <= 0)
                continue;

            index = 0;

            blocksPool.transform.DOMoveY(-animationStrength, pulseAnimationDuration).SetLoops(2, LoopType.Yoyo);

            foreach (BlockParent block in blockParents)
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
        bullet.transform.rotation = block.bulletPrefab.transform.rotation;
        bullet.transform.localScale = block.bulletPrefab.transform.localScale * 1.5f;
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

    private void ReleaseBullet(GameObject bullet)
    {
        bulletPool.Release(bullet);
    }
}
