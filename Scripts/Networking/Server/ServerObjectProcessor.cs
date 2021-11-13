using FlatBuffers;

using XRMultiplayer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using XRMultiplayer.Networking;
using XRMultiplayer.Models;
using LiteNetLib;

namespace XRMultiplayer.Networking.SERVER.Processors
{
    public class ServerObjectProcessor : BaseProcessor, IServerProcessor
    {
        public new Queue<NetworkObjectData> IncomingMessages { get; set; } = new Queue<NetworkObjectData>();

        public new Queue<NetworkObjectData> OutgoingMessages { get; set; } = new Queue<NetworkObjectData>();

        
        private Server server;

        public override MessageTypes MessageType { get { return MessageTypes.ObjectTransform; } }

        [Inject]
        public void Init(Server server)
        {
            this.server = server;
            server.PeerConnectedEvent += Server_PeerConnectedEvent;
        }

        private void Server_PeerConnectedEvent(NetPeer newPeer)
        {
            var changedObjs = dataManager.Objects.Select(x => x.Value);
            for (int j = 0; j < changedObjs.Count(); j++)
            {
                var objectToSync = changedObjs.ElementAt(j);
                objectToSync.LastSentPos = objectToSync.networkObjectData.Pos;
                var wrapperPacket = objectToSync.networkObjectData.Serialize();
                wrapperPacket.deliveryMethod = DeliveryMethod.ReliableUnordered;
                server.SendTo(wrapperPacket, newPeer);
            }
        }

        public override bool AddOutMessage(BaseMessageType objectToSend)
        {
            throw new NotImplementedException();
        }

        public override bool AddInMessage(byte[] message, NetPeer player)
        {
            NetworkObjectData objectTransform = new NetworkObjectData(message);           
            IncomingMessages.Enqueue(objectTransform);
            return true;
        }

        public override void ProcessIncoming()
        {
            while (IncomingMessages.Any())
            {
                var transformMsg = IncomingMessages.Dequeue();
                if (dataManager.Objects.ContainsKey(transformMsg.ObjectID))
                {
                    var objectTransform = dataManager.Objects[transformMsg.ObjectID];
                    objectTransform.networkObjectData = transformMsg;
                    objectTransform.gameObject.GetComponent<NetworkObject>().OwnerID = transformMsg.OwnerID;
                }
            }
        }

        public override void ProcessOutgoing()
        {
            while (OutgoingMessages.Any())
            {
                var postMsg = OutgoingMessages.Dequeue();
            }

            var changedObjs = dataManager.Objects.Where(x => x.Value.IsPositionChanged()).Select(x => x.Value);
            for (int j = 0; j < changedObjs.Count(); j++)
            {
                var objectToSync = changedObjs.ElementAt(j);
                objectToSync.LastSentPos = objectToSync.networkObjectData.Pos;
                server.SendToAll(objectToSync.networkObjectData.Serialize());
            }
        }

    }
}
