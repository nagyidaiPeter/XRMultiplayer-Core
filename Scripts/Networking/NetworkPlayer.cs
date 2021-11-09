
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

        public float Negate = 40;
        public float MinCurl = 10;
        public float MaxCurl = 100;

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

        private void Curl(Transform parent, float curlage, bool negative = false)
        {
            if (parent.GetComponent<FingerData>() is FingerData fingerData)
            {
                if (negative)
                {
                    parent.localEulerAngles = new Vector3(Mathf.Clamp(Negate - curlage, MinCurl, MaxCurl), 0, 0);
                }
                else
                {
                    parent.localEulerAngles = -new Vector3(Mathf.Clamp(Negate - curlage, MinCurl, MaxCurl), 0, 0);
                }
            }

            foreach (Transform child in parent)
            {
                Curl(child, curlage, negative);
            }
        }

        void Update()
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, playerData.playerTransform.Pos, InterVel * Time.deltaTime);
            transform.localRotation = playerData.playerTransform.Rot;
            transform.localEulerAngles -= playerData.playerTransform.QrRotationOffset;

            if (RH != null)
            {
                RH.localPosition = Vector3.Lerp(RH.localPosition, playerData.playerTransform.RHPos, HandInterVel * Time.deltaTime);
                RH.localRotation = playerData.playerTransform.RHRot;
                RH.gameObject.SetActive(playerData.playerTransform.RHActive);

                RH.GetComponent<LineRenderer>().SetPosition(1, playerData.playerTransform.RHPointerPos);

                Curl(RPinky, (byte.MaxValue / (1 + playerData.playerTransform.RHFingers.Pinky)));
                Curl(RRing, (byte.MaxValue / (1 + playerData.playerTransform.RHFingers.Ring)));
                Curl(RMiddle, (byte.MaxValue / (1 + playerData.playerTransform.RHFingers.Middle)));
                Curl(RIndex, (byte.MaxValue / (1 + playerData.playerTransform.RHFingers.Index)));
                Curl(RThumb, (byte.MaxValue / (Negate + playerData.playerTransform.RHFingers.Thumb) * 15), true);
            }

            if (LH != null)
            {
                LH.localPosition = Vector3.Lerp(LH.localPosition, playerData.playerTransform.LHPos, HandInterVel * Time.deltaTime);
                LH.localRotation = playerData.playerTransform.LHRot;
                LH.gameObject.SetActive(playerData.playerTransform.LHActive);

                LH.GetComponent<LineRenderer>().SetPosition(1, playerData.playerTransform.LHPointerPos);

                Curl(LPinky, (byte.MaxValue / (1 + playerData.playerTransform.LHFingers.Pinky)), true);
                Curl(LRing, (byte.MaxValue / (1 + playerData.playerTransform.LHFingers.Ring)), true);
                Curl(LMiddle, (byte.MaxValue / (1 + playerData.playerTransform.LHFingers.Middle)), true);
                Curl(LIndex, (byte.MaxValue / (1 + playerData.playerTransform.LHFingers.Index)), true);
                Curl(LThumb, (byte.MaxValue / (Negate + playerData.playerTransform.LHFingers.Thumb) * 15), true);
            }
        }



        public class Factory : PlaceholderFactory<string, NetworkPlayer>
        {
        }
    }
}
