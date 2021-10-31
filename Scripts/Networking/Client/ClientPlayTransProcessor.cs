using FlatBuffers;
using hololensMultiplayer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using hololensMultiplayer.Networking;
using hololensMultiplayer.Models;
using hololensMulti;
using hololensMultiModels;
using LiteNetLib;

namespace Assets.Scripts.SERVER.Processors
{
    public class ClientPlayTransProcessor : BaseProcessor
    {
        public new Queue<PlayerTransform> IncomingMessages { get; set; } = new Queue<PlayerTransform>();

        public new Queue<PlayerTransform> OutgoingMessages { get; set; } = new Queue<PlayerTransform>();

        [Inject]
        private Client client;

        [Inject]
        private NetworkPlayer.Factory playerFactory;

        public override bool AddInMessage(byte[] message, NetPeer player)
        {
            var transformList = PlayerTransform.DeserializeStack(message);
            foreach (var playerTransform in transformList)
            {
                IncomingMessages.Enqueue(playerTransform);
            }
            return true;
        }

        public override bool AddOutMessage(BaseMessageType objectToSend)
        {
            if (objectToSend is PlayerTransform playerTransform)
            {
                OutgoingMessages.Enqueue(playerTransform);
                return true;
            }

            return false;
        }

        public override void ProcessIncoming()
        {
            while (IncomingMessages.Any())
            {
                var transformMsg = IncomingMessages.Dequeue();

                if (transformMsg.SenderID == 0 || transformMsg.SenderID == dataManager.LocalPlayerID)
                    continue;

                if (dataManager.Players.ContainsKey(transformMsg.SenderID) && dataManager.Players[transformMsg.SenderID].playerObject != null)
                {
                    var player = dataManager.Players[transformMsg.SenderID];
                    player.playerTransform = transformMsg;
                }
                else
                {
                    if (!dataManager.Players.ContainsKey(transformMsg.SenderID))
                    {
                        PlayerData newPlayer = new PlayerData();
                        newPlayer.ID = transformMsg.SenderID;

                        var networkPlayer = playerFactory.Create("Players/PlayerHead");
                        var newPlayerGO = networkPlayer.gameObject;
                        newPlayerGO.SetActive(true);
                        newPlayerGO.transform.parent = GameObject.Find("NetworkSpace").transform;
                        newPlayer.playerObject = newPlayerGO;
                        newPlayer.playerTransform = transformMsg;
                        networkPlayer.playerData = newPlayer;
                        dataManager.Players.Add(newPlayer.ID, newPlayer);
                    }
                    else
                    {
                        PlayerData newPlayer = dataManager.Players[transformMsg.SenderID];

                        var networkPlayer = playerFactory.Create("Players/PlayerHead");
                        var newPlayerGO = networkPlayer.gameObject;
                        newPlayerGO.SetActive(true);
                        newPlayerGO.transform.parent = GameObject.Find("NetworkSpace").transform;
                        newPlayer.playerObject = newPlayerGO;
                        newPlayer.playerTransform = transformMsg;
                        networkPlayer.playerData = newPlayer;
                        dataManager.Players[newPlayer.ID] = newPlayer;
                    }
                }
            }
        }

        public override void ProcessOutgoing()
        {
            while (OutgoingMessages.Any())
            {
                var posMsg = OutgoingMessages.Dequeue();
                client.Send(posMsg.Serialize());
            }
        }
    }
}
