using XRMultiplayer;

using UnityEngine;
using Zenject;
using static XRMultiplayer.Networking.NetworkObject;
using XRMultiplayer.Networking.CLIENT.Processors;
using System.Collections;
using XRMultiplayer.Networking;

namespace XRMultiplayer
{
    public class ObjectHandler : MonoBehaviour
    {
        public NetworkSettingsObject networkSettings;

        [Inject]
        private DataManager dataManager;

        [Inject]
        private ObjectFactory objectFactory;

        [Inject]
        private ClientObjectProcessor objectProcessor;

        private void Start()
        {
            foreach (Transform child in transform)
            {
                NetworkObject networkObject = objectFactory.InitExistingNetworkObject(child.gameObject);
                dataManager.Objects.Add(networkObject.objectData.networkObjectData.ObjectID, networkObject.objectData);
            }

            StartCoroutine(SendData());
            StartCoroutine(ApplyData());
        }

        public void SpawnObject(string name)
        {
            var networkObject = objectFactory.Create(Resources.Load($"Objects/{name}"));
            var newPlayerGO = networkObject.gameObject;
            newPlayerGO.SetActive(true);
            dataManager.Objects.Add(networkObject.objectData.networkObjectData.ObjectID, networkObject.objectData);
        }

        private IEnumerator ApplyData()
        {
            while (true)
            {
                foreach (var networkObject in dataManager.Objects.Values)
                {
                    if (networkObject.networkObjectData.OwnerID != dataManager.LocalPlayerID)
                    {
                        float interpolVal = Time.deltaTime * networkSettings.ObjectInterpolation;
                        networkObject.gameObject.transform.localPosition = Vector3.Lerp(networkObject.gameObject.transform.localPosition, networkObject.networkObjectData.Pos, interpolVal);
                        networkObject.gameObject.transform.localRotation = Quaternion.Lerp(networkObject.gameObject.transform.localRotation, networkObject.networkObjectData.Rot, interpolVal);
                        networkObject.gameObject.transform.localScale = Vector3.Lerp(networkObject.gameObject.transform.localScale, networkObject.networkObjectData.Scale, interpolVal);
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator SendData()
        {
            while (true)
            {
                foreach (var networkObject in dataManager.Objects.Values)
                {
                    if (networkObject.networkObjectData.OwnerID == dataManager.LocalPlayerID)
                    {
                        networkObject.networkObjectData.OwnerID = dataManager.LocalPlayerID;
                        networkObject.networkObjectData.SenderID = dataManager.LocalPlayerID;

                        networkObject.networkObjectData.Pos = networkObject.gameObject.transform.localPosition;
                        networkObject.networkObjectData.Rot = networkObject.gameObject.transform.localRotation;
                        networkObject.networkObjectData.Scale = networkObject.gameObject.transform.localScale;
                        objectProcessor.AddOutMessage(networkObject.networkObjectData);
                    }
                }

                yield return new WaitForSeconds(networkSettings.NetworkRefreshRate);
            }
        }
    }
}
