
using hololensMultiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using hololensMultiplayer.Networking;
using hololensMultiplayer.Models;
using LiteNetLib;

namespace Assets.Scripts.SERVER.Processors
{
    public class ServerDisconnectProcessor : BaseProcessor
    {
        public new Queue<DisconnectMessage> IncomingMessages { get; set; } = new Queue<DisconnectMessage>();
        public new Queue<DisconnectMessage> OutgoingMessages { get; set; } = new Queue<DisconnectMessage>();

        [Inject]
        private Server server;

        public override void ProcessIncoming()
        {
            while (IncomingMessages.Any())
            {
                var dcMsg = IncomingMessages.Dequeue();
                var player = dataManager.Players[dcMsg.DisconnectedUserID];

                dataManager.Players.Remove(player.ID);
                Transform.Destroy(player.playerObject);
                OutgoingMessages.Enqueue(dcMsg);
            }
        }

        public override void ProcessOutgoing()
        {
            while (OutgoingMessages.Any())
            {
                var dcMsg = OutgoingMessages.Dequeue();
                server.SendToAll(dcMsg.Serialize());
            }
        }

        public override bool AddInMessage(byte[] message, NetPeer player)
        {
            DisconnectMessage disconnectMessage = new DisconnectMessage(message);
            IncomingMessages.Enqueue(disconnectMessage);
            return true;
        }

        public override bool AddOutMessage(BaseMessageType objectToSend)
        {
            if (objectToSend is DisconnectMessage dcMsg)
            {
                OutgoingMessages.Enqueue(dcMsg);
                return true;
            }

            return false;
        }
    }
}
