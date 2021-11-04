using XRMultiplayer.Models;
using UnityEngine;

using Zenject;
using XRMultiplayer.Networking;

namespace XRMultiplayer
{
    public class LocalPlayer : MonoBehaviour
    {
        public Transform networkSpace;

        [Inject]
        private DataManager dataManager;

        private ClientHandler client;

        public Transform RH, LH;

        void Start()
        {
            client = FindObjectOfType<ClientHandler>(true);
        }

        void Update()
        {
            if (client.IsConnected && dataManager.LocalPlayer != null &&
                dataManager.LocalPlayer.playerObject != null && dataManager.LocalPlayer.playerObject.activeSelf)
            {
                dataManager.LocalPlayer.playerTransform.SenderID = dataManager.LocalPlayerID;

                dataManager.LocalPlayer.playerTransform.Pos = networkSpace.InverseTransformPoint(transform.position);
                dataManager.LocalPlayer.playerTransform.Rot = transform.localRotation;
                dataManager.LocalPlayer.playerTransform.QrRotationOffset = networkSpace.eulerAngles;

                if (RH != null)

                {
                    dataManager.LocalPlayer.playerTransform.RHFingers = RH.GetComponent<HandTracker>().handState;
                    dataManager.LocalPlayer.playerTransform.RHPos = transform.InverseTransformPoint(RH.position);
                    dataManager.LocalPlayer.playerTransform.RHRot = RH.localRotation;
                    dataManager.LocalPlayer.playerTransform.RHActive = RH.GetComponent<HandTracker>().IsTracked;
                }

                if (LH != null)
                {
                    dataManager.LocalPlayer.playerTransform.LHFingers = LH.GetComponent<HandTracker>().handState;
                    dataManager.LocalPlayer.playerTransform.LHPos = transform.InverseTransformPoint(LH.position);
                    dataManager.LocalPlayer.playerTransform.LHRot = LH.localRotation;
                    dataManager.LocalPlayer.playerTransform.LHActive = LH.GetComponent<HandTracker>().IsTracked;
                }

                client.MessageProcessors[MessageTypes.PlayerTransform].AddOutMessage(dataManager.LocalPlayer.playerTransform);
            }
        }
    }

}