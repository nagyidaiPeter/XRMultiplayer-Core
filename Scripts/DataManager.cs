using System.Collections.Generic;

namespace hololensMultiplayer
{
    public class DataManager
    {
        public Dictionary<int, PlayerData> Players = new Dictionary<int, PlayerData>();
        public Dictionary<int, ObjectData> Objects = new Dictionary<int, ObjectData>();
        public byte LocalPlayerID;

        public PlayerData LocalPlayer
        {
            get
            {
                return Players.ContainsKey(LocalPlayerID) ? Players[LocalPlayerID] : null;
            }
        }

        public bool Welcomed = false;


        public delegate void StartingServer();
        public event StartingServer StartingServerEvent;
        private bool _isServer = false;
        public bool IsServer
        {
            get
            {
                return _isServer;
            }

            set
            {
                _isServer = value;
                if (_isServer)
                    StartingServerEvent?.Invoke();
            }
        }

        public DataManager()
        {

        }
    }
}
