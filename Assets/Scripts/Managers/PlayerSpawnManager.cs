using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spectrum
{
    public class PlayerSpawnManager : MonoBehaviour
    {
        public Vector3 GetLobbySpawnPosition(int playerID)
        {
            return this.transform.GetChild(Constants.SpawnTypeIndex.Lobby).GetChild(playerID).position;
        }

        public Vector3 GetGameSpawnPosition(int playerID)
        {
            return this.transform.GetChild(Constants.SpawnTypeIndex.Game).GetChild(playerID).position;
        }
    }
}