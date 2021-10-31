using Assets.Scripts.SERVER.Processors;
using hololensMultiplayer.Models;
using hololensMultiplayer.Networking;
using hololensMultiplayer.Packets;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace hololensMultiplayer
{
    public class Client : NetManager
    {
        public readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        public readonly EventBasedNetListener listener;

        public delegate void ReceivedUnconnectedServerResponse(IPEndPoint serverEndpoint);
        public event ReceivedUnconnectedServerResponse ServerBroadcastResponseEvent;

        public delegate void ConnectedToServer();
        public event ConnectedToServer ConnectedToServerEvent;

        public bool IsConnected { get; private set; }

        private NetworkSettingsObject networkSettings;

        public Client(EventBasedNetListener listener) : base(listener)
        {
            this.listener = listener;
            BroadcastReceiveEnabled = true;
            UnconnectedMessagesEnabled = true;
        }

        public new void Start(NetworkSettingsObject networkSettings)
        {
            this.networkSettings = networkSettings;
            listener.PeerConnectedEvent += PeerConnected;
            listener.PeerDisconnectedEvent += PeerDisconnected;
            listener.NetworkReceiveEvent += NetworkDataReceived;
            listener.NetworkReceiveUnconnectedEvent += OnNetworkReceiveUnconnected;

            base.Start(networkSettings.ClientPort);
        }

        public new void Stop()
        {
            listener.PeerConnectedEvent -= PeerConnected;
            listener.PeerDisconnectedEvent -= PeerDisconnected;
            listener.NetworkReceiveEvent -= NetworkDataReceived;

            base.Stop();
        }

        private void NetworkDataReceived(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader, peer);
        }

        private void PeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Debug.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
            IsConnected = false;
        }

        private void PeerConnected(NetPeer peer)
        {
            Debug.Log("[CLIENT] We connected to " + peer.EndPoint);
            IsConnected = true;
            ConnectedToServerEvent?.Invoke();
            listener.NetworkReceiveUnconnectedEvent -= OnNetworkReceiveUnconnected;
        }

        public void Send(WrapperPacket wrapperPacket)
        {
           FirstPeer.Send(netPacketProcessor.Write(wrapperPacket), (byte)wrapperPacket.UdpChannel, wrapperPacket.deliveryMethod);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.Broadcast && reader.GetString(networkSettings.AdvertisementID.Length) == networkSettings.AdvertisementID)
            {
                ServerBroadcastResponseEvent?.Invoke(remoteEndPoint);
            }
        }

    }
}