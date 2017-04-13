using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Spectrum
{
    public class CustomNetworkManager : NetworkManager
    {
        // Static singleton property
        public static CustomNetworkManager Instance { get; private set; }

        [SerializeField]
        private GameObject VRPlayerPrefab;
        [SerializeField]
        private GameObject nonVRPlayerPrefab;

        public List<NetworkVRPlayer> ConnectedPlayers { get; private set; }

        public NetworkVRPlayer GetPlayerById(int id)
        {
            return ConnectedPlayers[id];
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            Debug.Log("OnClientConnect");

            ClientScene.Ready(conn);
            ClientScene.AddPlayer(0);
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            // overriding without base to avoid this error:
            //http://answers.unity3d.com/questions/991552/unet-a-connection-has-already-been-set-as-ready-th.html
        }

        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
        {
            Debug.Log("OnServerAddPlayer");
            GameObject newPlayer = Instantiate(VRPlayerPrefab);

            /* I was trying to test networking with 1 VR and 1 nonVR client
             * Unfortunately, the approach below doesn't work, as the host controls the setting.
             * Needs a different approach or extra wrapper
            if (UnityEngine.VR.VRSettings.enabled)
            {
                newPlayer = Instantiate(VRPlayerPrefab);
            }
            else
            {
                newPlayer = Instantiate(nonVRPlayerPrefab);
            }
            */
            NetworkServer.AddPlayerForConnection(conn, newPlayer.gameObject, playerControllerId);
        }


        public void RegisterNetworkPlayer(NetworkVRPlayer newPlayer)
        {
            Debug.Log("New player joined");

            ConnectedPlayers.Add(newPlayer);

            UpdatePlayerIDs();
        }

        private void UpdatePlayerIDs()
        {
            for (int i = 0; i < ConnectedPlayers.Count; ++i)
            {
                ConnectedPlayers[i].SetPlayerID(i);
            }
        }

        public bool AllPlayersReady()
        {
            if (ConnectedPlayers.Count < 1)
            {
                return false;
            }

            for (int i = 0; i < ConnectedPlayers.Count; ++i)
            {
                if (!ConnectedPlayers[i].ReadyStatus)
                {
                    return false;
                }
            }

            return true;
        }

        public void ClearAllReadyStates()
        {
            for (int i = 0; i < ConnectedPlayers.Count; ++i)
            {
                NetworkVRPlayer player = ConnectedPlayers[i];
                if (player != null)
                {
                    player.ClearReady();
                }
            }
        }

        #region Unity Methods
        protected virtual void Awake()
        {   // There can be only one.
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                ConnectedPlayers = new List<NetworkVRPlayer>();
            }
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
        #endregion
    }
}