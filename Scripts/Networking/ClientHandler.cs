using XRMultiplayer.Models;
using XRMultiplayer.Packets;
using LiteNetLib;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Zenject;
using XRMultiplayer.Networking.CLIENT.Processors;
using System;
using System.Linq;

namespace XRMultiplayer.Networking
{
    public class ClientHandler : MonoBehaviour
    {
        public bool ClientIsServer = false;

        public Client client;
        private DataManager dataManager;

        public NetworkSettingsObject networkSettings;

        public Dictionary<MessageTypes, IProcessor> MessageProcessors = new Dictionary<MessageTypes, IProcessor>();

        [Inject]
        public void Init(Client client, DataManager dataManager, DiContainer _container)
        {
            this.client = client;
            this.dataManager = dataManager;

            this.client.netPacketProcessor.SubscribeReusable<WrapperPacket, NetPeer>(OnPacketReceived);

            //Get all client message processors filtered with namespace
            Type[] processors = typeof(IClientProcessor).Assembly.GetTypes()
              .Where(t => t.Namespace != null)
              .Where(t => t.Namespace.StartsWith("XRMultiplayer.Networking.CLIENT", StringComparison.Ordinal))
              .Where(t => t.IsSubclassOf(typeof(BaseProcessor)))
              .Where(t => !t.IsAbstract)
              .ToArray();

            foreach (var proc in processors)
            {
                if (_container.Resolve(proc) is IProcessor resolvedProc)
                {
                    MessageProcessors.Add(resolvedProc.MessageType, resolvedProc);
                }
            }
        }

        private void OnPacketReceived(WrapperPacket wrapperPacket, NetPeer peer)
        {
            MessageProcessors[wrapperPacket.messageType].AddInMessage(wrapperPacket.packetData, peer);
        }

        public bool IsConnected { get { return client.IsConnected; } }

        void Start()
        {
            client.Start(networkSettings);
            client.ServerBroadcastResponseEvent += Client_ServerBroadcastResponseEvent;
            StartCoroutine(ClientUpdate());
        }

        public void Connect(string address)
        {
            if (!dataManager.IsServer)
            {
                CleanNetworkObjects();
            }

            var split = address.Split(':');
            client.Connect(split[0], int.Parse(split[1]), networkSettings.AppKey);
            Debug.Log($"Connecting to {address}");
        }

        public void Connect(System.Net.IPEndPoint serverEndpoint)
        {
            if (!dataManager.IsServer)
            {
                CleanNetworkObjects();
            }

            client.Connect(serverEndpoint, networkSettings.AppKey);

            Debug.Log($"Connecting to {serverEndpoint}");
        }

        private void CleanNetworkObjects()
        {
            foreach (var netObj in dataManager.Objects)
            {
                Destroy(netObj.Value.gameObject);
            }
            dataManager.Objects.Clear();
        }

        public void Disconnect()
        {
            client.DisconnectAll();
        }

        private IEnumerator ClientUpdate()
        {
            while (true)
            {
                client.PollEvents();

                foreach (var handlerPair in MessageProcessors)
                {
                    handlerPair.Value.ProcessIncoming();
                    handlerPair.Value.ProcessOutgoing();
                }

                yield return new WaitForSeconds(networkSettings.NetworkRefreshRate);
            }
        }


        private void Client_ServerBroadcastResponseEvent(System.Net.IPEndPoint serverEndpoint)
        {
            Connect(serverEndpoint);
            client.ServerBroadcastResponseEvent -= Client_ServerBroadcastResponseEvent;
            client.DisconnectedFromServerEvent += Client_DisconnectedFromServerEvent;
        }

        private void Client_DisconnectedFromServerEvent()
        {
            client.ServerBroadcastResponseEvent += Client_ServerBroadcastResponseEvent;
        }

        private void OnDestroy()
        {
            Disconnect();
            client.Stop();
            StopAllCoroutines();
        }
    }

}