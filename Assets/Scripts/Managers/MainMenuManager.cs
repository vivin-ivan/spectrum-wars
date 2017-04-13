using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Spectrum
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject localVRSystem;

        private CustomNetworkManager networkManager;

        void Start()
        {
            var localVRSystemInstance = GameObject.FindGameObjectsWithTag(Constants.Tag.LocalVRSystem);

            if (UnityEngine.VR.VRSettings.enabled)
            {
                if (localVRSystemInstance.Length == 0)
                {
                    DontDestroyOnLoad(Instantiate(localVRSystem, Vector3.zero, Quaternion.identity));
                }

                StartCoroutine(InitInputListeners());
            }

            networkManager = GameObject.FindGameObjectWithTag(Constants.Tag.NetworkManager).GetComponent<CustomNetworkManager>();
        }

        IEnumerator InitInputListeners()
        {
            // This gives a new local VR rig some time to init
            yield return new WaitForSeconds(0.1f);

            //Reusing events and buttons meant for NetworkVRPlayer here to save time.
            LocalVRManager.Instance.GetInputManager().EitherReadyButtonPressed += DoReadyButtonPressed; //Touchpad to host
            LocalVRManager.Instance.GetInputManager().CubeHandTriggerPressed += DoTriggerPressed; // Trigger to join LAN
            LocalVRManager.Instance.GetInputManager().WeaponTriggerPressed += DoTriggerPressed; // Trigger to join LAN
        }

        void DoReadyButtonPressed()
        {
            networkManager.StartHost();
        }

        void DoTriggerPressed()
        {
            networkManager.StartClient();
        }

        void Update()
        {
            // for testing purposes - if no VR
            if (Input.GetKeyDown(KeyCode.Space))
            {
                networkManager.StartHost();
            }
            if (Input.GetKeyDown(KeyCode.J))
            {
                networkManager.StartClient();
            }
        }

        void OnDisable()
        {
            if (UnityEngine.VR.VRSettings.enabled)
            {
                LocalVRManager.Instance.GetInputManager().EitherReadyButtonPressed -= DoReadyButtonPressed; //Touchpad to host
                LocalVRManager.Instance.GetInputManager().CubeHandTriggerPressed -= DoTriggerPressed; // Trigger to join LAN
                LocalVRManager.Instance.GetInputManager().WeaponTriggerPressed -= DoTriggerPressed; // Trigger to join LAN
            }
        }
    }
}