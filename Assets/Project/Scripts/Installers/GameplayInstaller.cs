using UnityEngine;
using Zenject;

public class GameplayInstaller : MonoInstaller
{
    [SerializeField] private BlockManager blockManager;
    [SerializeField] private TileManager tileManager;
    [SerializeField] private MergePlatformsManager mergePlatformsManager;

    public override void InstallBindings()
    {
        if (blockManager == null)
        {
            Debug.LogError("BlockManager is not assigned in the GameplayInstaller.");
            return;
        }
        Container.Bind<BlockManager>().FromInstance(blockManager).AsSingle();
        Container.Bind<TileManager>().FromInstance(tileManager).AsSingle();
        Container.Bind<MergePlatformsManager>().FromInstance(mergePlatformsManager).AsSingle();
        Container.Bind<Camera>().FromInstance(Camera.main).AsSingle();
        
    }
}
