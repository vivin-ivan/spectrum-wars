using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

namespace Spectrum
{
    public delegate void KeyBindEventHandler();

    public class InputManager : MonoBehaviour
    {
        public event KeyBindEventHandler EitherReadyButtonPressed;
        public event KeyBindEventHandler CubeHandTriggerPressed;
        public event KeyBindEventHandler WeaponTriggerPressed;

        private VRTK_ControllerEvents cubeHandEvents;
        private VRTK_ControllerEvents weaponHandEvents;

        void Start()
        {
            cubeHandEvents = LocalVRManager.Instance.GetLeftControllerScripts().GetComponent<VRTK_ControllerEvents>();
            weaponHandEvents = LocalVRManager.Instance.GetRightControllerScripts().GetComponent<VRTK_ControllerEvents>();

            cubeHandEvents.TouchpadPressed += new ControllerInteractionEventHandler(DoPadPressed);
            cubeHandEvents.TriggerClicked += new ControllerInteractionEventHandler(DoCubeHandTriggerClicked);
            weaponHandEvents.TouchpadPressed += new ControllerInteractionEventHandler(DoPadPressed);
            weaponHandEvents.TriggerClicked += new ControllerInteractionEventHandler(DoWeaponTriggerClicked);
        }

        private void DoPadPressed(object sender, ControllerInteractionEventArgs e)
        {
            if (EitherReadyButtonPressed != null)
                EitherReadyButtonPressed();
        }

        private void DoCubeHandTriggerClicked(object sender, ControllerInteractionEventArgs e)
        {
            if (CubeHandTriggerPressed != null)
                CubeHandTriggerPressed();
        }

        private void DoWeaponTriggerClicked(object sender, ControllerInteractionEventArgs e)
        {
            if (WeaponTriggerPressed != null)
                WeaponTriggerPressed();
        }

        void OnDisable()
        {
            cubeHandEvents.StartMenuPressed -= new ControllerInteractionEventHandler(DoPadPressed);
            weaponHandEvents.StartMenuPressed -= new ControllerInteractionEventHandler(DoPadPressed);
            weaponHandEvents.TriggerClicked -= new ControllerInteractionEventHandler(DoWeaponTriggerClicked);
        }
    }
}
