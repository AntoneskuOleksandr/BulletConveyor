using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject]
    public void Construct(BulletGenerator bulletGenerator)
    {
        StartCoroutine(bulletGenerator.StartBulletGeneration());
    }
}