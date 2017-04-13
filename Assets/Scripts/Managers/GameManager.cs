using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Spectrum
{
    public class GameManager : NetworkBehaviour
    {
        // Static singleton property
        public static GameManager Instance { get; private set; }
        
        // Game state vars
        public enum GameState
        {
            Lobby,
            PreGameSetup,
            Game,
            PostGame
        }

        private GameState activeState;
        public GameState ActiveState
        {
            get { return activeState; }
        }

        // Score Manager ref
        private ScoreManager scoreManager;

        // Time vars
        private float serverTime = 0f;
        private float gameStartTime = 0f;
        [SerializeField]
        private float roundTimeInSeconds = 30f;

        // Lobby vars
        private bool isCountingDown = false;
        private float preGameDelayTime = 5f;

        // Target vars
        [SerializeField]
        private GameObject targetPrefab;
        private List<GameObject> targetList = new List<GameObject>();

        // Color vars
        private int[] randomColorArray;

        public int[] RandomColorArray
        {
            get { return randomColorArray; }
        }

        #region Unity Methods
        void Awake()
        {   // There can be only one.
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            Instance = this;
        }

        [ServerCallback]
        void Start()
        {
            activeState = GameState.Lobby;
            scoreManager = GetComponent<ScoreManager>();
        }

        [ServerCallback]
        void Update()
        {
            serverTime += Time.deltaTime;

            HandleStateUpdate();
        }
        #endregion

        void HandleStateUpdate()
        {
            switch (activeState)
            {
                case GameState.Lobby:
                    LobbyUpdate();
                    break;

                case GameState.PreGameSetup:
                    PreGameSetupUpdate();
                    break;

                case GameState.Game:
                    GameUpdate();
                    break;

                case GameState.PostGame:
                    PostGameUpdate();
                    break;

                default: Debug.LogError("Handle State Update default case.");
                    break;
            }
        }

        void LobbyUpdate()
        {
            if (CustomNetworkManager.Instance.AllPlayersReady() && !isCountingDown)
            {
                isCountingDown = true;
                StartCoroutine(PreGameDelay());
            }
        }

        //TODO: This could be another state
        IEnumerator PreGameDelay()
        {
            Debug.Log("Game starts in 5!");
            yield return new WaitForSeconds(preGameDelayTime);
            
            // If all players are still ready, let's go!
            if (CustomNetworkManager.Instance.AllPlayersReady())
            {
                activeState = GameState.PreGameSetup;
                CustomNetworkManager.Instance.ClearAllReadyStates();
            }

            isCountingDown = false;
        }

        void PreGameSetupUpdate()
        {
            gameStartTime = serverTime;
            scoreManager.ClearScoreList();

            int currentPlayerCount = CustomNetworkManager.Instance.ConnectedPlayers.Count;
            randomColorArray = Utilities.ColorRandomizer.GetNewRandomColors(currentPlayerCount);

            // Assign colors to players and place them in the game.
            for (int id = 0; id < currentPlayerCount; ++id)
            {
                NetworkVRPlayer player = CustomNetworkManager.Instance.ConnectedPlayers[id];
                if (player != null)
                {
                    player.activeColorIndex = randomColorArray[id];
                    player.SetPlayerInGame(true);
                    player.RpcMoveToGameSpawn();
                    scoreManager.RegisterPlayerToScoreList();
                }
            }

            // Spawn 3 times as many targets as players
            for (int i = 0; i < currentPlayerCount * 3; ++i)
            {
                Vector3 targetPos = GetTargetSpawnManager().GetRandomTargetSpawn();
                GameObject newTarget = Instantiate(targetPrefab, targetPos, Quaternion.identity);
                newTarget.GetComponent<Target>().UpdateActiveColorIndex();

                NetworkServer.Spawn(newTarget);
                targetList.Add(newTarget);
            }
            
            activeState = GameState.Game;
        }

        void GameUpdate()
        {
            // Change to post game after 30 seconds
            if (serverTime > gameStartTime + roundTimeInSeconds)
            {
                activeState = GameState.PostGame;
            }
        }

        void PostGameUpdate()
        {
            int currentPlayerCount = CustomNetworkManager.Instance.ConnectedPlayers.Count;

            // Reset everything, send everyone back to the lobby.
            randomColorArray = new int[0];
            for (int id = 0; id < currentPlayerCount; ++id)
            {
                NetworkVRPlayer player = CustomNetworkManager.Instance.ConnectedPlayers[id];
                if (player != null && player.InGameStatus)
                {
                    player.RpcClearColor();
                    player.SetPlayerInGame(false);
                    player.RpcMoveToLobbySpawn();
                }
            }

            // Remove all Targets from the game
            foreach (GameObject target in targetList)
            {
                NetworkServer.UnSpawn(target);
                DestroyObject(target);
            }
            targetList = new List<GameObject>();

            activeState = GameState.Lobby;
        }

        public PlayerSpawnManager GetPlayerSpawnManager()
        {
            return this.transform.GetComponentInChildren<PlayerSpawnManager>();
        }

        public TargetSpawnManager GetTargetSpawnManager()
        {
            return this.transform.GetComponentInChildren<TargetSpawnManager>();
        }
    }
}