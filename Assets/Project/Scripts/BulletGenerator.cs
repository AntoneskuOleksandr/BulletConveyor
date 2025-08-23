using System.Collections;
using UnityEngine;
using Zenject;

public class BulletGenerator : MonoBehaviour
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float generationInterval = 1f; // Интервал генерации пуль

    private BoardManager _boardManager;

    [Inject]
    public void Construct(BoardManager boardManager)
    {
        _boardManager = boardManager;
    }

    public IEnumerator StartBulletGeneration()
    {
        while (true)
        {
            yield return new WaitForSeconds(generationInterval); // Интервал генерации
            Vector3 generationPosition = transform.position; // Позиция генерации

            GenerateBullets(generationPosition);
        }
    }

    private void GenerateBullets(Vector3 position)
    {
        int bulletsToGenerate = _boardManager.GetBulletsToGenerateCount();

        Debug.Log("Generating bullets; Count: " + bulletsToGenerate);

        for (int i = 0; i < bulletsToGenerate; i++)
        {
            GenerateBullet(transform.position);
        }
    }

    private void GenerateBullet(Vector3 position)
    {
        Instantiate(bulletPrefab, transform.position, Quaternion.identity);
    }
}
