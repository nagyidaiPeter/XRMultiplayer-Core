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
using LiteNetLib;

namespace Assets.Scripts.SERVER.Processors
{
    public class ServerObjectProcessor : BaseProcessor
    {
        public new Queue<ObjectTransform> IncomingMessages { get; set; } = new Queue<ObjectTransform>();

        public new Queue<ObjectTransform> OutgoingMessages { get; set; } = new Queue<ObjectTransform>();

        
        private Server server;

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
                objectToSync.LastSentPos = objectToSync.objectTransform.Pos;
                var wrapperPacket = objectToSync.objectTransform.Serialize();
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
            ObjectTransform objectTransform = new ObjectTransform(message);           
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
                    objectTransform.objectTransform = transformMsg;
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
                objectToSync.LastSentPos = objectToSync.objectTransform.Pos;
                server.SendToAll(objectToSync.objectTransform.Serialize());
            }
        }

    }
}
