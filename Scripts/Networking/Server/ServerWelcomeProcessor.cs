
using hololensMultiplayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using hololensMultiplayer.Networking;
using hololensMultiplayer.Models;
using hololensMulti;
using FlatBuffers;
using LiteNetLib;

namespace Assets.Scripts.SERVER.Processors
{
    public class ServerWelcomeProcessor : BaseProcessor
    {
        public new Queue<Welcome> IncomingMessages { get; set; } = new Queue<Welcome>();
        public new Queue<Welcome> OutgoingMessages { get; set; } = new Queue<Welcome>();

        [Inject]
        private Server netServer;

        [Inject]
        private NetworkPlayer.Factory playerFactory;

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

                if (!dataManager.Players.ContainsKey(welcomeMsg.SenderID))
                {
                    PlayerData newPlayer = new PlayerData();
                    newPlayer.ID = welcomeMsg.SenderID;

                    var networkPlayer = playerFactory.Create("Players/PlayerHead");
                    var newPlayerGO = networkPlayer.gameObject;
                    newPlayerGO.SetActive(true);
                    newPlayerGO.transform.parent = GameObject.Find("NetworkSpace").transform;
                    newPlayer.playerObject = newPlayerGO;
                    newPlayer.Name = welcomeMsg.Name;
                    networkPlayer.playerData = newPlayer;
                    dataManager.Players.Add(newPlayer.ID, newPlayer);
                }
            }
        }

        public override void ProcessOutgoing()
        {
            while (OutgoingMessages.Any())
            {
                //var msg = netServer.CreateMessage();
                var welcomeMsg = OutgoingMessages.Dequeue();

                //Inform others about new player
                //PlayerInfo playerInfo = new PlayerInfo();
                //playerInfo.SenderID = welcomeMsg.SenderID;
                //playerInfo.Name = welcomeMsg.Name;

                //var body = JsonConvert.SerializeObject(playerInfo);

                //msg.Write((byte)MessageTypes.PlayerInfo);
                //msg.Write(body.Length);
                //msg.Write(body);

                //netServer.SendToAll(msg, NetDeliveryMethod.ReliableOrdered);
            }
        }
    }
}
