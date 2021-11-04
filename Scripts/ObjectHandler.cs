using XRMultiplayer;

using UnityEngine;
using Zenject;
using static XRMultiplayer.Networking.NetworkObject;
using XRMultiplayer.Networking.CLIENT.Processors;

namespace XRMultiplayer
{
    public class ObjectHandler : MonoBehaviour
    {
        [Inject]
        private DataManager dataManager;

        [Inject]
        private ObjectFactory objectFactory;


        private void Start()
        {
            foreach (Transform child in transform)
            {
                objectFactory.InitExistingNetworkObject(child.gameObject);
            }
        }

        public void SpawnObject(string name)
        {
            var networkObject = objectFactory.Create(Resources.Load($"Objects/{name}"));
            var newPlayerGO = networkObject.gameObject;
            newPlayerGO.SetActive(true);
            dataManager.Objects.Add(networkObject.objectData.objectTransform.ObjectID, networkObject.objectData);
        }
    }
}
