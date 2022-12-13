using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FlowWordsTutorial
{
    public struct ScoreStruct
    {
        public string Name;
        public int Score;

        public ScoreStruct(string name, int score)
        {
            Name = name;
            Score = score;
        }
    }

    /// <summary>
    /// Represents the High Scores panel
    /// </summary>
    public class HighScoresPanel : MonoBehaviour
    {
        // UI Hookups
        public TMPro.TextMeshProUGUI PlayerStats;
        public TMPro.TextMeshProUGUI GlobalStats;

        public RectTransform[] m_DistributionBars;

        private void OnEnable()
        {
            //Start loading scores in the background when this panel is enabled.
            StartCoroutine(LoadScores());
        }

        /// <summary>
        /// Coroutine to load scores asynchronously.
        /// </summary>
        private IEnumerator LoadScores()
        {
            // set loading text
            GlobalStats.text = "Global High Scores\n\nLoading...";
            PlayerStats.text = "Player Stats\n\nLoading...";

            // get data from chain
            yield return FlowInterface.Instance.LoadHighScoresFromChain(OnLoadScoresSuccess, OnLoadScoresFailure);
        }

        /// <summary>
        /// Function called after successfully loading high score data
        /// </summary>
        /// <param name="globalScores">A list of ScoreStructs containing the scores</param>
        /// <param name="playerCumulativeScore">The players cumulative score</param>
        /// <param name="playerCurrentWinningStreak">The players current winning streak</param>
        /// <param name="playerMaxWinningStreak">The players max winning streak</param>
        /// <param name="playerGuessDistribution">
        /// Array showing how many times the player has won.  Index is number of guesses, value is number of times the player
        /// won using that many guesses.
        /// </param>
        private void OnLoadScoresSuccess(List<ScoreStruct> globalScores, uint playerCumulativeScore, uint playerCurrentWinningStreak, uint playerMaxWinningStreak, uint[] playerGuessDistribution)
        {
            // populate text
            GlobalStats.text = "Global High Scores\n\n";
            foreach (ScoreStruct score in globalScores)
            {
                GlobalStats.text += ($"{score.Name} - {score.Score}pts\n");
            }

            PlayerStats.text = "Player Stats\n\n";
            PlayerStats.text += $"Total Score:    {playerCumulativeScore}\n";
            PlayerStats.text += $"Current Streak: {playerCurrentWinningStreak}\n";
            PlayerStats.text += $"Best Streak:    {playerMaxWinningStreak}\n\n";
            PlayerStats.text += $"Guess Distribution\n";

            uint highest = 0;

            foreach (uint score in playerGuessDistribution)
            {
                if (score > highest)
                {
                    highest = score;
                }

                PlayerStats.text += ($"{score} ");
            }

            uint pixelsPer = 0;

            if (highest != 0)
            {
                pixelsPer = 100 / highest;
            }

            for (int i = 0; i < 6; i++)
            {
                m_DistributionBars[i].sizeDelta = new Vector2(30, Mathf.Max(playerGuessDistribution[i] * pixelsPer, 3));
            }
        }

        /// <summary>
        /// Function called when loading scores fails
        /// </summary>
        private void OnLoadScoresFailure()
        {
            GlobalStats.text = "Error loading high scores.";
            PlayerStats.text = "";
        }

        /// <summary>
        /// Set the status text of the High Score Panel.
        /// NOTE:  
        /// </summary>
        /// <remarks>
        /// Currently there is no status area of the panel and this should never be called.
        /// This is provided for future expansion if needed and will throw an exception if called.
        /// </remarks>
        /// <param name="status">The status text to be displayed in the status area</param>
        /// <exception cref="System.NotImplementedException"></exception>
        internal void SetStatus(string status)
        {
            throw new System.NotImplementedException();
        }


        /// <summary>
        /// Handler that closes the high scores panel when the close button is clicked
        /// </summary>
        public void CloseHighScorePanelClicked()
        {
            GameManager.Instance.CloseHighScores();
        }
    }
}
