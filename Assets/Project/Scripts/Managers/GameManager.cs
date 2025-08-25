using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    private BlockManager _blockManager;
    private TileManager _tileManager;
    private TileGenerator _tileGenerator;
    private BoardManager _boardManager;
    private MergePlatformsManager _mergePlatformsManager;
    private BulletGenerator _bulletGenerator;

    [Inject]
    public void Construct(BlockManager blockManager, TileManager tileManager, TileGenerator tileGenerator, 
        BoardManager boardManager, MergePlatformsManager mergePlatformsManager, BulletGenerator bulletGenerator)
    {
        _blockManager = blockManager;
        _tileManager = tileManager;
        _tileGenerator = tileGenerator;
        _boardManager = boardManager;
        _mergePlatformsManager = mergePlatformsManager;
        _bulletGenerator = bulletGenerator;

        StartGame();
    }

    public void StartGame()
    {
        _tileGenerator.GenerateGrid(6, 6);
        _tileManager.Initialize();
        _bulletGenerator.Initialize();

        StartCoroutine(_bulletGenerator.StartBulletGeneration());
    }
}