using Assets.Scripts.SERVER.Processors;

using FlatBuffers;

using hololensMultiplayer.Models;
using hololensMultiplayer.Networking;
using hololensMultiplayer.Packets;
using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace hololensMultiplayer
{
    public class Server : NetManager
    {
        public readonly NetPacketProcessor netPacketProcessor = new NetPacketProcessor();
        public readonly EventBasedNetListener listener;

        private byte ConnectedPlayers = 0;

        public delegate void NewPeerConnection(NetPeer newPeer);
        public event NewPeerConnection PeerConnectedEvent;

        public Server(EventBasedNetListener listener) : base(listener)
        {
            this.listener = listener;
            BroadcastReceiveEnabled = true;
            UnconnectedMessagesEnabled = true;            
        }

        public new void Stop()
        {
            listener.PeerConnectedEvent -= PeerConnected;
            listener.NetworkReceiveEvent -= NetworkDataReceived;

            base.Stop();
        }

        public new void Start(int port)
        {
            listener.PeerConnectedEvent += PeerConnected;
            listener.NetworkReceiveEvent += NetworkDataReceived;

            base.Start(port);
        }

        private void PeerConnected(NetPeer peer)
        {
            if (ConnectedPlayers > byte.MaxValue)
                ConnectedPlayers = byte.MinValue;;

            ConnectedPlayers++;
            var newPlayer = new PlayerData();
            newPlayer.connection = peer;
            newPlayer.ID = ConnectedPlayers;

            //dataManager.Players.Add(newPlayer.ID, newPlayer);

            //Welcome player
            Welcome welcomeMsg = new Welcome();
            welcomeMsg.SenderID = ConnectedPlayers;
            welcomeMsg.Name = newPlayer.Name;

            var msgBody = welcomeMsg.Serialize();

            peer.Send(netPacketProcessor.Write(msgBody), DeliveryMethod.ReliableOrdered);

            PeerConnectedEvent?.Invoke(peer);
        }

        private void NetworkDataReceived(NetPeer peer, NetDataReader reader, DeliveryMethod deliveryMethod)
        {
            netPacketProcessor.ReadAllPackets(reader, peer);
        }


        public void SendToAll(WrapperPacket packet)
        {
            SendToAll(netPacketProcessor.Write(packet), (byte)packet.UdpChannel, packet.deliveryMethod);
        }

        public void SendTo(WrapperPacket packet, NetPeer peer)
        {
            peer.Send(netPacketProcessor.Write(packet), (byte)packet.UdpChannel, packet.deliveryMethod);
        }

    }
}
