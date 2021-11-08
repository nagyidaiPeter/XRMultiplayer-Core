
using System.Collections;

using UnityEngine;

using Zenject;

namespace XRMultiplayer.Networking
{
    public class NetworkPlayer : MonoBehaviour
    {
        public float InterVel = 20;
        public float HandInterVel = 20;
        public PlayerData playerData = null;

        public Transform RH, LH;

        [Header("Right fingers")]
        public Transform RPinky;
        public Transform RRing;
        public Transform RMiddle;
        public Transform RIndex;
        public Transform RThumb;

        [Header("Left fingers")]
        public Transform LPinky;
        public Transform LRing;
        public Transform LMiddle;
        public Transform LIndex;
        public Transform LThumb;

        [Inject]
        private DataManager dataManager;

        void Start()
        {
            StartCoroutine(DisableLocalPlayer());
        }

        private IEnumerator DisableLocalPlayer()
        {
            Debug.Log($"Local player is null: {dataManager.LocalPlayer == null}");
            while (dataManager.LocalPlayer == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            Debug.Log($"Got local player: {dataManager.LocalPlayer != null}");
            if (dataManager.LocalPlayer.ID == playerData.ID)
            {
                dataManager.Players[playerData.ID].playerObject.GetComponent<MeshRenderer>().enabled = false;
                foreach (var renderer in dataManager.Players[playerData.ID].playerObject.GetComponentsInChildren<Renderer>())
                {
                    renderer.enabled = false;
                }
            }
        }

        private void Curl(Transform parent, float curlage)
        {
            if (parent.GetComponent<FingerData>() is FingerData fingerData)
                parent.localEulerAngles = new Vector3(45 * curlage, 0, 0);

            foreach (Transform child in parent)
            {
                Curl(child, curlage);
            }
        }

        void Update()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, playerData.playerTransform.Pos, InterVel * Time.deltaTime);            
            transform.localRotation = Quaternion.Lerp(transform.localRotation, playerData.playerTransform.Rot, InterVel * Time.deltaTime);
            transform.localEulerAngles -= playerData.playerTransform.QrRotationOffset;

            if (RH != null)
            {
                RH.localPosition = Vector3.Lerp(RH.localPosition, playerData.playerTransform.RHPos, HandInterVel * Time.deltaTime);
                RH.localRotation = Quaternion.Lerp(RH.localRotation, playerData.playerTransform.RHRot, HandInterVel * Time.deltaTime);
                RH.gameObject.SetActive(playerData.playerTransform.RHActive);

                Curl(RPinky, -(byte.MaxValue / (1 + playerData.playerTransform.RHFingers.Pinky)));
                Curl(RRing, -(byte.MaxValue / (1 + playerData.playerTransform.RHFingers.Ring)));
                Curl(RMiddle, -(byte.MaxValue / (1 + playerData.playerTransform.RHFingers.Middle)));
                Curl(RIndex, -(byte.MaxValue / (1 + playerData.playerTransform.RHFingers.Index)));
                Curl(RThumb, (byte.MaxValue / (1 + playerData.playerTransform.RHFingers.Thumb)));
            }

            if (LH != null)
            {
                LH.localPosition = Vector3.Lerp(LH.localPosition, playerData.playerTransform.LHPos, HandInterVel * Time.deltaTime);
                LH.localRotation = Quaternion.Lerp(LH.localRotation, playerData.playerTransform.LHRot, HandInterVel * Time.deltaTime);
                LH.gameObject.SetActive(playerData.playerTransform.LHActive);

                Curl(LPinky, (byte.MaxValue / (1 + playerData.playerTransform.LHFingers.Pinky)));
                Curl(LRing, (byte.MaxValue / (1 + playerData.playerTransform.LHFingers.Ring)));
                Curl(LMiddle, (byte.MaxValue / (1 + playerData.playerTransform.LHFingers.Middle)));
                Curl(LIndex, (byte.MaxValue / (1 + playerData.playerTransform.LHFingers.Index)));
                Curl(LThumb, (byte.MaxValue / (1 + playerData.playerTransform.LHFingers.Thumb)));
            }
        }



        public class Factory : PlaceholderFactory<string, NetworkPlayer>
        {
        }
    }
}
