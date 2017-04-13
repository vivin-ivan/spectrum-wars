using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace Spectrum
{
    public class LocalVRManager : MonoBehaviour
    {
        // Static singleton property
        public static LocalVRManager Instance { get; private set; }

        void Awake()
        {   // There can be only one.
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            Instance = this;
        }

        public void MoveTo(Vector3 newPosition)
        {
            this.transform.position = newPosition;
        }

        public InputManager GetInputManager()
        {
            return this.GetComponent<InputManager>();
        }

        public Transform GetHMD()
        {
            return VRTK_SDKManager.instance.actualHeadset.transform;
        }

        public Transform GetLeftHand()
        {
            return VRTK_SDKManager.instance.actualLeftController.transform;
        }

        public Transform GetRightHand()
        {
            return VRTK_SDKManager.instance.actualRightController.transform;
        }

        public Transform GetLeftControllerScripts()
        {
            return VRTK_SDKManager.instance.scriptAliasLeftController.transform;
        }

        public Transform GetRightControllerScripts()
        {
            return VRTK_SDKManager.instance.scriptAliasRightController.transform;
        }
    }
}