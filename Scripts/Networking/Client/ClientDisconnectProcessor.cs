using XRMultiplayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using XRMultiplayer.Networking;
using XRMultiplayer.Models;
using FlatBuffers;

using LiteNetLib;

namespace XRMultiplayer.Networking.CLIENT.Processors
{
    public class ClientDisconnectProcessor : BaseProcessor, IClientProcessor
    {
        public new Queue<DisconnectMessage> IncomingMessages { get; set; } = new Queue<DisconnectMessage>();
        public new Queue<DisconnectMessage> OutgoingMessages { get; set; } = new Queue<DisconnectMessage>();

        [Inject]
        private Server server;

        public override MessageTypes MessageType { get { return MessageTypes.Disconnect; } }

        public override void ProcessIncoming()
        {
            while (IncomingMessages.Any())
            {
                var dcMsg = IncomingMessages.Dequeue();
                Debug.Log($"Player {dcMsg.DisconnectedUserID} disconnected..");
                var player = dataManager.Players.FirstOrDefault(x => x.Key == dcMsg.DisconnectedUserID).Value;

                dataManager.Players.Remove(player.ID);
                GameObject.Destroy(player.playerObject);
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
            ByteBuffer bb = new ByteBuffer(message);
            DisconnectFB disconnectFB = DisconnectFB.GetRootAsDisconnectFB(bb);
            DisconnectMessage dcMsg = new DisconnectMessage();

            dcMsg.SenderID = disconnectFB.PlayerID;
            dcMsg.DisconnectedUserID = disconnectFB.PlayerID;

            IncomingMessages.Enqueue(dcMsg);
            return true;
        }

        public override bool AddOutMessage(BaseMessageType objectToSend)
        {
            return true;
        }
    }
}
