using Zenject;

public class WeaponModifierInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Weapon Modifiers
        Container.Bind<EveryNthDamageBoostModifier>().AsSingle();

        // Resolver
        Container.Bind<WeaponModifierResolver>().AsSingle();
    }
}
