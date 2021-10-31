using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Assets.Scripts.SERVER.Processors;

using hololensMultiplayer;
using hololensMultiplayer.Models;

using UnityEngine;

using Zenject;

public class NetworkObject : MonoBehaviour
{
    public byte OwnerID = 0;
    public float InterVel = 35;
    public ObjectData objectData = null;
    public Transform qrPos;
    public int RefreshRate = 30;

    [Inject]
    private DataManager dataManager;

    [Inject]
    private ClientObjectProcessor objectProcessor;

    void Start()
    {
        qrPos = transform.parent.Find("QR");
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

        public override NetworkObject Create(UnityEngine.Object prefab)
        {
            if (int.MaxValue == ID)
            {
                ID = 0;
            }

            ID++;

            NetworkObject instance;

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
