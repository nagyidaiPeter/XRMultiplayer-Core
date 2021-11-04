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
        public float InterVel = 35;
        public ObjectData objectData = null;
        public int RefreshRate = 30;

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
            if (dataManager == null || objectProcessor == null || objectData == null)
            {
                enabled = false;
                return;
            }
            StartCoroutine(SendData());
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
            objectData.objectTransform.OwnerID = OwnerID;
            objectData.objectTransform.SenderID = OwnerID;

            objectData.objectTransform.Pos = transform.localPosition;
            objectData.objectTransform.Rot = transform.localRotation;
            objectData.objectTransform.Scale = transform.localScale;

            objectProcessor.AddOutMessage(objectData.objectTransform);
        }

        private IEnumerator SendData()
        {
            while (true)
            {
                if (OwnerID != dataManager.LocalPlayerID)
                {
                    transform.localPosition = objectData.objectTransform.Pos;
                    transform.localRotation = objectData.objectTransform.Rot;
                    transform.localScale = objectData.objectTransform.Scale;
                }
                else
                {
                    objectData.objectTransform.OwnerID = OwnerID;
                    objectData.objectTransform.SenderID = OwnerID;

                    objectData.objectTransform.Pos = transform.localPosition;
                    objectData.objectTransform.Rot = transform.localRotation;
                    objectData.objectTransform.Scale = transform.localScale;

                    objectProcessor.AddOutMessage(objectData.objectTransform);
                }
                yield return new WaitForSeconds(1 / RefreshRate);
            }
        }

        public void DisclaimObject()
        {
            OwnerID = 0;

            objectData.objectTransform.ObjectID = objectData.objectTransform.ObjectID;
            objectData.objectTransform.OwnerID = OwnerID;
            objectData.objectTransform.SenderID = OwnerID;

            objectData.objectTransform.ObjectType = objectData.objectTransform.ObjectType;
            objectData.objectTransform.Pos = transform.localPosition;
            objectData.objectTransform.Rot = transform.localRotation;
            objectData.objectTransform.Scale = transform.localScale;

            objectProcessor.AddOutMessage(objectData.objectTransform);
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

            public void InitExistingNetworkObject(GameObject netNetworkedGO)
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
                networkObject.objectData.objectTransform = new ObjectTransform();
                networkObject.objectData.objectTransform.Pos = netNetworkedGO.transform.localPosition;
                networkObject.objectData.objectTransform.Rot = netNetworkedGO.transform.localRotation;
                networkObject.objectData.objectTransform.Scale = netNetworkedGO.transform.localScale;
                networkObject.objectData.objectTransform.ObjectType = netNetworkedGO.name;
                networkObject.objectData.objectTransform.ObjectID = NextID();
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
                    instance.objectData.objectTransform = new ObjectTransform();
                    instance.objectData.objectTransform.ObjectType = prefab.name;
                    instance.objectData.objectTransform.ObjectID = ID;
                    return instance;
                }

                return null;
            }
        }
    }

}