using Assets.Scripts.SERVER.Processors;

using hololensMultiplayer;
using LiteNetLib;
using Zenject;

public class DependencyInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        InstallCoreDependencies();

        InstallClientDependencies();

        InstallServerDependencies();
    }

    public void InstallCoreDependencies()
    {
        Container.Bind<DataManager>().AsSingle().NonLazy();
        Container.Bind<EventBasedNetListener>().AsTransient();

        //Factories
        Container.BindFactory<string, NetworkPlayer, NetworkPlayer.Factory>().FromFactory<PrefabResourceFactory<NetworkPlayer>>();
        Container.BindFactory<UnityEngine.Object, NetworkObject, NetworkObject.ObjectFactory>().FromFactory<NetworkObject.ObjectFactory>();
    }

    public void InstallServerDependencies()
    {
        Container.Bind<Server>().AsSingle().NonLazy();

        //Server processors
        Container.Bind<ServerPlayTransProcessor>().AsSingle().NonLazy();
        Container.Bind<ServerWelcomeProcessor>().AsSingle().NonLazy();
        Container.Bind<ServerDisconnectProcessor>().AsSingle().NonLazy();
        Container.Bind<ServerObjectProcessor>().AsSingle().NonLazy();
    }

    public void InstallClientDependencies()
    {
        Container.Bind<Client>().AsSingle().NonLazy();

        //Client processors
        Container.Bind<ClientPlayTransProcessor>().AsSingle().NonLazy();
        Container.Bind<ClientWelcomeProcessor>().AsSingle().NonLazy();
        Container.Bind<ClientDisconnectProcessor>().AsSingle().NonLazy();
        Container.Bind<ClientObjectProcessor>().AsSingle().NonLazy();
    }
}