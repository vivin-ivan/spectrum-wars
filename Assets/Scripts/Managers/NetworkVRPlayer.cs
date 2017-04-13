using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Spectrum
{
    public class NetworkVRPlayer : NetworkBehaviour
    {
        private InputManager myInputManager;
        private GameManager myGameManager;
        private CustomNetworkManager myNetworkManager;

        private float timeSinceLastShot = 0f;           // to limit fire spam
        private bool isConsecutiveHit;                  // for scoring bonus

        // Sync Vars and associated calls
        #region Player ID
        // Players receive an ID based on their index in ConnectedPlayers list.
        [SyncVar]
        private int playerID = -1;

        [Server]
        public void SetPlayerID(int newPlayerID)
        {
            this.playerID = newPlayerID;
        }

        public int PlayerID
        {
            get { return playerID; }
        }
        #endregion

        #region Ready Status
        // Players can ready up in the lobby game state
        [SyncVar]
        private bool readyStatus = false;

        public bool ReadyStatus
        {
            get { return readyStatus; }
        }

        [Command]
        public void CmdSetReady()
        {
            if (GameManager.Instance.ActiveState == GameManager.GameState.Lobby)
            {
                readyStatus = readyStatus ? false : true;
                Debug.Log("Player ready status: " + readyStatus);
            }
        }

        [Server]
        public void ClearReady()
        {
            readyStatus = false;
        }
        #endregion

        #region Ingame Status
        //Whether or not this player is inside the game (as opposed to in the lobby)
        [SyncVar]
        private bool inGameStatus = false;

        public bool InGameStatus
        {
            get { return inGameStatus; }
        }

        [Server]
        public void SetPlayerInGame(bool status)
        {
            inGameStatus = status;
        }
        #endregion

        #region Active Color Index
        [SyncVar(hook = "UpdateColor")]
        public int activeColorIndex;

        void UpdateColor(int hook)
        {
            Color newColor = Utilities.ColorRandomizer.colorDictionary[hook];
            // Hard-coded hierarchy traversal - Child 0 is head, child 0 is the color sphere. 
            this.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = newColor;
            // Hard-coded hierarchy traversal - Child 1 is left hand, child 0 is the color cube. 
            this.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = newColor;
            Debug.Log("UpdateColor changed to " + hook);
        }

        [ClientRpc]
        public void RpcClearColor()
        {
            // Hard-coded hierarchy traversal - Child 0 is head, child 0 is the color sphere. 
            this.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.clear;
            // Hard-coded hierarchy traversal - Child 1 is left hand, child 0 is the color cube. 
            this.transform.GetChild(1).GetChild(0).GetComponent<MeshRenderer>().material.color = Color.clear;
        }
        #endregion

        void Start()
        {
            if (hasAuthority)
            {
                if (UnityEngine.VR.VRSettings.enabled)
                {
                    myInputManager = LocalVRManager.Instance.GetInputManager();

                    myInputManager.WeaponTriggerPressed += DoTriggerPressed;
                    myInputManager.EitherReadyButtonPressed += DoReadyButtonPressed;
                }
                else // for nonVR network testing 
                {
                    CmdSetReady();
                }

                Transform VRPlayerHead = this.transform.GetChild(0).GetChild(0);
                MeshRenderer[] childMeshRenderers = VRPlayerHead.GetComponentsInChildren<MeshRenderer>();
                VRPlayerHead.GetComponent<MeshRenderer>().enabled = false;
                foreach (MeshRenderer mr in childMeshRenderers)
                    mr.enabled = false;
            }

            myGameManager = GameManager.Instance;
        }

        [ServerCallback]
        void Update()
        {
            timeSinceLastShot += Time.deltaTime;
        }

        [Client]
        public override void OnStartClient()
        {
            if (myNetworkManager == null)
            {
                myNetworkManager = CustomNetworkManager.Instance;
            }

            base.OnStartClient();
            Debug.Log("Client network player start");

            myNetworkManager.RegisterNetworkPlayer(this);
        }

        [Command]
        public void CmdFireWeapon()
        {
            if (myGameManager.ActiveState == GameManager.GameState.Game && this.inGameStatus)
            {
                if (timeSinceLastShot > 0.5f)
                {
                    timeSinceLastShot = 0f;

                    // Do hit detection - this should be its own script but works for now
                    RaycastHit hit;
                    
                    // Hard-coded hierarchy traversal for muzzle location: Right Hand, Pistol, Muzzle
                    Transform muzzle = this.transform.GetChild(2).GetChild(0).GetChild(0);

                    Debug.DrawRay(muzzle.position, muzzle.forward, Color.blue, 1f);

                    if (Physics.Raycast(muzzle.position, muzzle.forward, out hit, 100f))
                    {
                        // If player hits a target
                        if (hit.transform.tag == Constants.Tag.Target)
                        {
                            // Mixing in score calculation here - this should be separate
                            // 3 points for hitting your color
                            // 2 for consecutive hit
                            // 1 for enemy color
                            // -10 for getting hit by projectile

                            int scoreToAdd = 0;
                            Target target = hit.transform.GetComponent<Target>();

                            if (activeColorIndex == target.activeColorIndex)
                            {
                                scoreToAdd += 3;
                            }
                            else
                            {
                                target.ShootBack(this.transform.GetChild(0).position);
                                scoreToAdd += 1;
                            }

                            target.UpdateActiveColorIndex();

                            if (isConsecutiveHit)
                            {
                                scoreToAdd += 2;
                            }
                            else
                            {
                                isConsecutiveHit = true;
                            }

                            GameManager.Instance.GetComponent<ScoreManager>().AddToPlayerScore(this.playerID, scoreToAdd);
                        }
                        else
                        {
                            isConsecutiveHit = false;
                        }
                    }
                    else
                    {
                        isConsecutiveHit = false;
                    }

                    RpcFireWeapon();
                }
            }
        }

        [ClientRpc]
        private void RpcFireWeapon()
        {
            //TODO: sync event would be much neater
            // Hard-coded hierarchy traversal to get audio source and play weapon fire
            this.transform.GetChild(2).GetChild(0).GetComponent<AudioSource>().Play();
        }

        [ClientRpc]
        public void RpcMoveToLobbySpawn()
        {
            if (hasAuthority)
            {
                if (UnityEngine.VR.VRSettings.enabled)
                    LocalVRManager.Instance.MoveTo(myGameManager.GetPlayerSpawnManager().GetLobbySpawnPosition(this.playerID));
            }
        }

        [ClientRpc]
        public void RpcMoveToGameSpawn()
        {
            if (hasAuthority)
            {
                if (UnityEngine.VR.VRSettings.enabled)
                    LocalVRManager.Instance.MoveTo(myGameManager.GetPlayerSpawnManager().GetGameSpawnPosition(this.playerID));
            }
        }
        
        private void DoTriggerPressed()
        {
            CmdFireWeapon();
        }

        private void DoReadyButtonPressed()
        {
            CmdSetReady();
        }

        void OnDestroy()
        {
            if (hasAuthority)
            {
                myInputManager.WeaponTriggerPressed -= DoTriggerPressed;
                myInputManager.EitherReadyButtonPressed -= DoReadyButtonPressed;
            }
        }
    }
}