using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    [SerializeField] private BlockManager blockManager;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private TileGenerator tileGenerator;
    [SerializeField] private BoardManager boardManager;
    [SerializeField] private MergePlatformsManager mergePlatformsManager;
    [SerializeField] private BulletGenerator bulletGenerator;

    public override void InstallBindings()
    {
        Container.Bind<BlockManager>().FromInstance(blockManager).AsSingle();
        Container.Bind<TileManager>().FromInstance(tileManager).AsSingle();
        Container.Bind<TileGenerator>().FromInstance(tileGenerator).AsSingle();
        Container.Bind<BoardManager>().FromInstance(boardManager).AsSingle();
        Container.Bind<MergePlatformsManager>().FromInstance(mergePlatformsManager).AsSingle();
        Container.Bind<BulletGenerator>().FromInstance(bulletGenerator).AsSingle();
        Container.Bind<Camera>().FromInstance(Camera.main).AsSingle();
    }
}
