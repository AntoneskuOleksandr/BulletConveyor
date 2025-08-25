using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    private BulletGenerator _bulletGenerator;
    private TileGenerator _tileGenerator;

    [Inject]
    public void Construct(BulletGenerator bulletGenerator, TileGenerator tileGenerator)
    {
        _bulletGenerator = bulletGenerator;
        _tileGenerator = tileGenerator;
        StartGame();
    }

    public void StartGame()
    {
        _tileGenerator.GenerateGrid(6, 6);
        StartCoroutine(_bulletGenerator.StartBulletGeneration());
    }
}