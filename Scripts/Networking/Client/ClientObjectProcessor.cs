using FlatBuffers;
using XRMultiplayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using XRMultiplayer.Networking;
using XRMultiplayer.Models;
using LiteNetLib;

namespace XRMultiplayer.Networking.CLIENT.Processors
{
    public class ClientObjectProcessor : BaseProcessor
    {
        public new Queue<NetworkObjectData> IncomingMessages { get; set; } = new Queue<NetworkObjectData>();

        public new Queue<NetworkObjectData> OutgoingMessages { get; set; } = new Queue<NetworkObjectData>();

        [Inject]
        private Client client;

        [Inject]
        private NetworkObject.ObjectFactory objectFactory;

        public override bool AddInMessage(byte[] message, NetPeer peer)
        {
            NetworkObjectData objectTransform = new NetworkObjectData(message);
            IncomingMessages.Enqueue(objectTransform);
            return true;
        }

        public override bool AddOutMessage(BaseMessageType objectToSend)
        {
            if (objectToSend is NetworkObjectData ObjectTransform)
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
                    objectTransform.networkObjectData = objectTransformMsg;
                }
                else
                {
                    ObjectData newObject = new ObjectData();
                    var networkObject = objectFactory.Create(Resources.Load($"Objects/{objectTransformMsg.ObjectType}"));
                    var newPlayerGO = networkObject.gameObject;
                    newPlayerGO.SetActive(true);
                    newObject.gameObject = newPlayerGO;
                    newObject.networkObjectData = objectTransformMsg;
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
