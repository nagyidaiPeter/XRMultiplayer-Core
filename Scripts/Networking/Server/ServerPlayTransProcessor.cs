using FlatBuffers;

using hololensMultiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using hololensMultiplayer.Networking;
using hololensMultiplayer.Models;
using hololensMulti;
using hololensMultiModels;
using LiteNetLib;
using hololensMultiplayer.Packets;

namespace Assets.Scripts.SERVER.Processors
{
    public class ServerPlayTransProcessor : BaseProcessor
    {
        public new Queue<PlayerTransform> IncomingMessages { get; set; } = new Queue<PlayerTransform>();

        public new Queue<PlayerTransform> OutgoingMessages { get; set; } = new Queue<PlayerTransform>();

        [Inject]
        private Server server;

        public override bool AddOutMessage(BaseMessageType objectToSend)
        {
            throw new NotImplementedException();
        }

        public override bool AddInMessage(byte[] message, NetPeer player)
        {
            PlayerTransform playerTransform = new PlayerTransform(message);
            IncomingMessages.Enqueue(playerTransform);
            return true;
        }

        public override void ProcessIncoming()
        {
            while (IncomingMessages.Any())
            {
                var transformMsg = IncomingMessages.Dequeue();
                if (dataManager.Players.ContainsKey(transformMsg.SenderID))
                {
                    var player = dataManager.Players[transformMsg.SenderID];
                    player.playerTransform = transformMsg; 
                }
            }
        }

        public override void ProcessOutgoing()
        {
            while (OutgoingMessages.Any())
            {
                var postMsg = OutgoingMessages.Dequeue();
            }

            var playerTransforms = dataManager.Players.Values.Select(x => x.playerTransform).ToList();
            var transformPackets = PlayerTransform.Serialize(playerTransforms);
            foreach(WrapperPacket packet in transformPackets)
            {                
                server.SendToAll(packet);
            }
        }

    }
}
