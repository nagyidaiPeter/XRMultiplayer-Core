
using hololensMultiModels;
using LiteNetLib;
using UnityEngine;

namespace hololensMultiplayer
{
    public class PlayerData
    {
        public byte ID;
        public NetPeer connection;
        public string Name = "Player";

        public bool IsWelcomed = false;

        public PlayerTransform playerTransform = new PlayerTransform();

        public GameObject playerObject;

        public PlayerData()
        {

        }
    }
}