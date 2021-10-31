using FlatBuffers;
using hololensMultiplayer;
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
    public class ClientObjectProcessor : BaseProcessor
    {
        public new Queue<ObjectTransform> IncomingMessages { get; set; } = new Queue<ObjectTransform>();

        public new Queue<ObjectTransform> OutgoingMessages { get; set; } = new Queue<ObjectTransform>();

        [Inject]
        private Client client;

        [Inject]
        private NetworkObject.ObjectFactory objectFactory;

        public override bool AddInMessage(byte[] message, NetPeer peer)
        {
            ObjectTransform objectTransform = new ObjectTransform(message);
            IncomingMessages.Enqueue(objectTransform);
            return true;
        }

        public override bool AddOutMessage(BaseMessageType objectToSend)
        {
            if (objectToSend is ObjectTransform ObjectTransform)
            {
                OutgoingMessages.Enqueue(ObjectTransform);
                return true;
            }

            return false;
        }

        public override void ProcessIncoming()
        {
            while (IncomingMessages.Any())
            {
                var objectTransformMsg = IncomingMessages.Dequeue();

                if (dataManager.IsServer)
                    continue;

                if (dataManager.Objects.ContainsKey(objectTransformMsg.ObjectID))
                {
                    var objectTransform = dataManager.Objects[objectTransformMsg.ObjectID];
                    objectTransform.objectTransform = objectTransformMsg;
                }
                else
                {
                    ObjectData newObject = new ObjectData();
                    var networkObject = objectFactory.Create(Resources.Load($"Objects/{objectTransformMsg.ObjectType}"));
                    var newPlayerGO = networkObject.gameObject;
                    newPlayerGO.SetActive(true);
                    newObject.gameObject = newPlayerGO;
                    newObject.objectTransform = objectTransformMsg;
                    networkObject.objectData = newObject;
                    dataManager.Objects.Add(objectTransformMsg.ObjectID, newObject);
                }
            }
        }

        public override void ProcessOutgoing()
        {
            while (OutgoingMessages.Any())
            {
                var posMsg = OutgoingMessages.Dequeue();
                posMsg.SenderID = dataManager.LocalPlayerID;
                client.Send(posMsg.Serialize());
            }
        }

    }
}
