using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Spectrum
{
    public class ScoreManager : NetworkBehaviour
    {
        [SerializeField]
        private GameObject scoreWall;

        private SyncListInt scoreList = new SyncListInt();

        private Text text;

        private StringBuilder sb;

        void Start()
        {
            scoreList.Callback = UpdateWallText;
            text = scoreWall.GetComponentInChildren<Text>();
        }

        [Server]
        public void RegisterPlayerToScoreList()
        {
            scoreList.Add(0);
        }

        [Server]
        public void AddToPlayerScore(int playerID, int scoreToAdd)
        {
            scoreList[playerID] += scoreToAdd;
        }

        [Server]
        public void ClearScoreList()
        {
            scoreList.Clear();
        }

        private void UpdateWallText(SyncListInt.Operation op, int index)
        {
            sb = new StringBuilder();
            sb.Append("Score: ").AppendLine();

            for(int i = 0; i < scoreList.Count; ++i)
            {
                sb.Append("Player ").Append(i).Append(": ").Append(scoreList[i]);
                sb.AppendLine();
            }

            text.text = sb.ToString();
        }
    }
}