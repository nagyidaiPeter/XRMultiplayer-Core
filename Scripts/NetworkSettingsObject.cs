using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkSettings", menuName = "Networking/NetworkSettings", order = 1)]
public class NetworkSettingsObject : ScriptableObject
{
    public int ServerPort = 12345;

    public int ClientPort = 12346;

    public float NetworkRefreshRate = 0.033333f;

    public int MaxConnections = 32;

    public string AdvertisementID = "xrM";

    public string AppKey = "xrMultiplayer";
}
