
using XRMultiplayer;
using System.Collections.Generic;
using System.Linq;
using Zenject;
using XRMultiplayer.Networking;
using XRMultiplayer.Models;
using FlatBuffers;
using LiteNetLib;
using UnityEngine;

namespace XRMultiplayer.Networking.CLIENT.Processors
{
    public class ClientWelcomeProcessor : BaseProcessor, IClientProcessor
    {
        public new Queue<Welcome> IncomingMessages { get; set; } = new Queue<Welcome>();
        public new Queue<Welcome> OutgoingMessages { get; set; } = new Queue<Welcome>();

        [Inject]
        private Client client;

        public override MessageTypes MessageType { get { return MessageTypes.Welcome; } }

        public override bool AddInMessage(byte[] message, NetPeer player)
        {
            Welcome welcome = new Welcome(message);
            IncomingMessages.Enqueue(welcome);
            return true;
        }
        public override bool AddOutMessage(BaseMessageType objectToSend)
        {
            if (objectToSend is Welcome welcomeMsg)
            {
                OutgoingMessages.Enqueue(welcomeMsg);
                return true;
            }
            return false;
        }

        public override void ProcessIncoming()
        {
            while (IncomingMessages.Any())
            {
                var welcomeMsg = IncomingMessages.Dequeue();

                Debug.Log($"Server welcomed us: {welcomeMsg.SenderID}");

                dataManager.LocalPlayerID = welcomeMsg.SenderID;
                dataManager.Players.Add(dataManager.LocalPlayerID, new PlayerData());

                dataManager.LocalPlayer.playerObject = GameObject.FindObjectOfType<LocalPlayer>().gameObject;

                OutgoingMessages.Enqueue(welcomeMsg);
            }
        }

        public override void ProcessOutgoing()
        {
            while (OutgoingMessages.Any())
            {             
                var welcomeMsg = OutgoingMessages.Dequeue();
                client.Send(welcomeMsg.Serialize());
            }
        }

    }
}
