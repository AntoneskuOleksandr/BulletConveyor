using System.Collections.Generic;
using UnityEngine;

public class BulletGenerator : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    private List<BlockParent> blocks;

    public void GenerateBullet(Vector3 position)
    {
        for (int i = 0; i < GetBulletsToGenerateCount(); i++)
            Instantiate(bulletPrefab, position, Quaternion.identity);
    }

    public void SetBlock(BlockParent block)
    {
        blocks.Add(block);
    }

    private int GetBulletsToGenerateCount()
    {
        int count = 0;
        foreach (var block in blocks)
        {
            if (block != null)
                count += block.bulletsToGenerate;
        }

        return count;
    }
}
