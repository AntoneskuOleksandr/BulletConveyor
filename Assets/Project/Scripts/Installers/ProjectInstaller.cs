using Zenject;
using UnityEngine;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private WeaponDatabaseSO weaponDatabase;

    public override void InstallBindings()
    {
        Container.BindInstance(weaponDatabase).AsSingle();

        Container.BindInterfacesAndSelfTo<DataManager>().AsSingle().NonLazy();

        Container.Bind<ICurrencyWriter>().FromResolveGetter<DataManager>(dm => dm.CurrencyWriter);

        Container.Bind<EconomyService>().AsSingle();
    }
}
