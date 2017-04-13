using UnityEngine;

namespace Spectrum
{
    enum Components { Head, LeftHand, RightHand }

    public class ComponentTracker : MonoBehaviour
    {
        //TODO: Should probably not have used the word "component" as it's a Unity term, oops.

        [SerializeField]
        private Components componentToTrack;

        private Transform componentTransform;
        bool isDisabled = false;

        void Start()
        {
            // This script is for local VR players only
            if (!this.transform.parent.GetComponent<NetworkVRPlayer>().hasAuthority 
                || !UnityEngine.VR.VRSettings.enabled)
            {
                this.GetComponent<ComponentTracker>().enabled = false;
                isDisabled = true;
                return;
            }

            componentTransform = GetComponentTransform();
        }

        void LateUpdate()
        {
            if (componentTransform != null)
            {
                transform.position = componentTransform.position;
                transform.rotation = componentTransform.rotation;
            }
            else
            {
                if (!isDisabled)
                    componentTransform = GetComponentTransform();
            }
        }

        private Transform GetComponentTransform()
        {
            switch (componentToTrack)
            {
                case Components.Head:
                    return LocalVRManager.Instance.GetHMD();

                case Components.LeftHand:
                    return LocalVRManager.Instance.GetLeftHand();

                case Components.RightHand:
                    return LocalVRManager.Instance.GetRightHand();

                default:
                    Debug.Log("ComponentToTrack default case");
                    return null;
            }
        }
    }
}