

using LiteNetLib;
using UnityEngine;

using XRMultiplayer.Models;

namespace XRMultiplayer
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