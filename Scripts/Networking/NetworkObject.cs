using System.Collections;
using System.Collections.Generic;
using System.Linq;

using XRMultiplayer.Networking.SERVER.Processors;

using XRMultiplayer;
using XRMultiplayer.Models;

using UnityEngine;

using Zenject;
using XRMultiplayer.Networking.CLIENT.Processors;

namespace XRMultiplayer.Networking
{
    public class NetworkObject : MonoBehaviour
    {
        public byte OwnerID = 0;
        public float InterVel = 20;
        public ObjectData objectData = null;

        private DataManager dataManager;
        private ClientObjectProcessor objectProcessor;

        [Inject]
        private void Init(DataManager dataManager, ClientObjectProcessor objectProcessor)
        {
            this.dataManager = dataManager;
            this.objectProcessor = objectProcessor;
        }

        private void Start()
        {

        }

        void Update()
        {

        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }

        public void ClaimObject()
        {
            OwnerID = dataManager.LocalPlayerID;
            objectData.networkObjectData.OwnerID = OwnerID;
            objectData.networkObjectData.SenderID = OwnerID;

            objectData.networkObjectData.Pos = transform.localPosition;
            objectData.networkObjectData.Rot = transform.localRotation;
            objectData.networkObjectData.Scale = transform.localScale;

            objectProcessor.AddOutMessage(objectData.networkObjectData);
        }

        public void DisclaimObject()
        {
            OwnerID = 0;

            objectData.networkObjectData.ObjectID = objectData.networkObjectData.ObjectID;
            objectData.networkObjectData.OwnerID = OwnerID;
            objectData.networkObjectData.SenderID = OwnerID;

            objectData.networkObjectData.ObjectType = objectData.networkObjectData.ObjectType;
            objectData.networkObjectData.Pos = transform.localPosition;
            objectData.networkObjectData.Rot = transform.localRotation;
            objectData.networkObjectData.Scale = transform.localScale;

            objectProcessor.AddOutMessage(objectData.networkObjectData);
        }

        public class ObjectFactory : PlaceholderFactory<UnityEngine.Object, NetworkObject>
        {
            public static int ID;

            readonly DiContainer _container;

            private static Dictionary<string, List<GameObject>> ObjectPools = new Dictionary<string, List<GameObject>>();

            public ObjectFactory(DiContainer container)
            {
                _container = container;

                foreach (var obj in Resources.LoadAll("Objects"))
                {
                    ObjectPools.Add(obj.name, new List<GameObject>());
                }
            }

            public void AddToPool(NetworkObject networkObject)
            {
                string ObjectKey = networkObject.name;
                if (!ObjectPools.ContainsKey(ObjectKey))
                {
                    ObjectPools.Add(ObjectKey, new List<GameObject>());
                }

                ObjectPools[ObjectKey].Add(networkObject.gameObject);
                networkObject.gameObject.SetActive(false);
            }

            public NetworkObject InitExistingNetworkObject(GameObject netNetworkedGO)
            {
                NetworkObject networkObject;
                if (netNetworkedGO.GetComponent<NetworkObject>() is NetworkObject originalComp)
                {
                    networkObject = originalComp;
                }
                else
                {
                    networkObject = _container.InstantiateComponent<NetworkObject>(netNetworkedGO);
                }

                networkObject.objectData = new ObjectData();
                networkObject.objectData.gameObject = netNetworkedGO;
                networkObject.objectData.networkObjectData = new NetworkObjectData();
                networkObject.objectData.networkObjectData.Pos = netNetworkedGO.transform.localPosition;
                networkObject.objectData.networkObjectData.Rot = netNetworkedGO.transform.localRotation;
                networkObject.objectData.networkObjectData.Scale = netNetworkedGO.transform.localScale;
                networkObject.objectData.networkObjectData.ObjectType = netNetworkedGO.name;
                networkObject.objectData.networkObjectData.ObjectID = NextID();

                return networkObject;
            }

            private int NextID()
            {
                if (int.MaxValue == ID)
                {
                    ID = 0;
                }

                return ID++;
            }

            public override NetworkObject Create(UnityEngine.Object prefab)
            {
                NetworkObject instance;
                NextID();

                if (prefab is GameObject gameObject)
                {
                    var prefabScript = gameObject.GetComponent<NetworkObject>();

                    if (ObjectPools[prefab.name].Any())
                    {
                        var newObject = ObjectPools[prefab.name].First();
                        ObjectPools[prefab.name].Remove(newObject);
                        instance = newObject.GetComponent<NetworkObject>();
                    }
                    else
                    {
                        instance = _container.InstantiatePrefabForComponent<NetworkObject>(prefab, new Vector3(0, 0, 0), new Quaternion(), null);
                        instance.transform.parent = GameObject.Find("NetworkSpace").transform;

                    }
                    instance.objectData = new ObjectData();
                    instance.objectData.gameObject = instance.gameObject;
                    instance.objectData.networkObjectData = new NetworkObjectData();
                    instance.objectData.networkObjectData.ObjectType = prefab.name;
                    instance.objectData.networkObjectData.ObjectID = ID;
                    return instance;
                }

                return null;
            }
        }
    }

}