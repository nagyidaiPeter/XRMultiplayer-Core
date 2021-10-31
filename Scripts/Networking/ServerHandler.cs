
using Assets.Scripts.SERVER.Processors;
using hololensMultiplayer;
using hololensMultiplayer.Models;
using hololensMultiplayer.Networking;
using hololensMultiplayer.Packets;
using LiteNetLib;
using LiteNetLib.Utils;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Zenject;

public class ServerHandler : MonoBehaviour
{
    private Server server;

    public bool IsServerRunning = false;

    public NetworkSettingsObject networkSettings;

    private DataManager dataManager;

    private Dictionary<MessageTypes, IProcessor> MessageProcessors = new Dictionary<MessageTypes, IProcessor>();

    [Inject]
    private NetworkObject.ObjectFactory objectFactory;

    [Inject]
    public void Init(Server server, DataManager dataManager,
            ServerPlayTransProcessor playerTransformProc, ServerWelcomeProcessor serverWelcomeProcessor,
            ServerDisconnectProcessor disconnectProcessor, ServerObjectProcessor objectProcessor)
    {
        this.server = server;
        this.dataManager = dataManager;

        //todo: This will get out of hand if we need more processors, rework this to collect all processors from assembly with reflection or similar solution
        MessageProcessors.Add(MessageTypes.PlayerTransform, playerTransformProc);
        MessageProcessors.Add(MessageTypes.Welcome, serverWelcomeProcessor);
        MessageProcessors.Add(MessageTypes.Disconnect, disconnectProcessor);
        MessageProcessors.Add(MessageTypes.ObjectTransform, objectProcessor);

        this.server.netPacketProcessor.SubscribeReusable<WrapperPacket, NetPeer>(OnPacketReceived);
    }

    private void OnPacketReceived(WrapperPacket wrapperPacket, NetPeer peer)
    {
        MessageProcessors[wrapperPacket.messageType].AddInMessage(wrapperPacket.packetData, peer);
    }

    public void StartServer()
    {
        if (!server.IsRunning)
        {
            Debug.Log("Starting server..");
            server.listener.PeerDisconnectedEvent += PeerDisconnected;
            server.listener.ConnectionRequestEvent += OnConnectionRequest;

            server.Start(networkSettings.ServerPort);
            IsServerRunning = true;
            dataManager.IsServer = true;

            foreach (var obj in Resources.LoadAll("Objects"))
            {
                GetComponent<ObjectSpawner>().SpawnObject(obj.name);
            }

            //GetComponent<ObjectSpawner>().SpawnObject("HumanHeart");

            StartCoroutine(ServerUpdate());
            StartCoroutine(BroadcastServer());
        }
    }

    private void OnDestroy()
    {
        StopServer();
    }

    public void StopServer()
    {
        Debug.Log("Stopping server..");
        IsServerRunning = false;
        dataManager.IsServer = false;
        StopAllCoroutines();
        server.Stop();

        server.listener.PeerDisconnectedEvent -= PeerDisconnected;
        server.listener.ConnectionRequestEvent -= OnConnectionRequest;

        for (int i = 0; i < dataManager.Objects.Count; i++)
        {
            var first = dataManager.Objects.ElementAt(i);
            objectFactory.AddToPool(first.Value.gameObject.GetComponent<NetworkObject>());
        }
        dataManager.Objects.Clear();
    }

    private void OnConnectionRequest(ConnectionRequest request)
    {
        if (server.ConnectedPeersCount < networkSettings.MaxConnections)
        {
            Debug.Log("New peer wants to connect!");
            request.AcceptIfKey(networkSettings.AppKey);
        }
        else
        {
            request.Reject();
        }
    }

    private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        DisconnectMessage disconnectMessage = new DisconnectMessage();
        disconnectMessage.DisconnectedUserID = dataManager.Players.FirstOrDefault(x => x.Value.connection == peer).Value.ID;
        MessageProcessors[MessageTypes.Disconnect].AddOutMessage(disconnectMessage);
    }

    private IEnumerator ServerUpdate()
    {
        while (IsServerRunning)
        {
            server.PollEvents();
            foreach (var processor in MessageProcessors)
            {
                try
                {
                    processor.Value.ProcessIncoming();
                    processor.Value.ProcessOutgoing();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex);
                }
            }

            yield return new WaitForSeconds(networkSettings.NetworkRefreshRate);
        }
    }

    private IEnumerator BroadcastServer()
    {
        while (server.IsRunning)
        {
            NetDataWriter writer = new NetDataWriter();
            writer.Put(networkSettings.AdvertisementID);
            server.SendBroadcast(writer, networkSettings.ClientPort);
            yield return new WaitForSeconds(0.5f);
        }
    }
}