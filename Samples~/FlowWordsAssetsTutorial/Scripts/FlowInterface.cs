using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FlowWordsTutorial
{
    public class FlowInterface : MonoBehaviour
    {
        // FLOW account object - set via Login screen.
        [Header("FLOW Account")]

        // The TextAssets containing Cadence scripts and transactions that will be used for the game.
        [Header("Scripts and Transactions")]
        [SerializeField] TextAsset loginTxn;
        [SerializeField] TextAsset getCurrentGameStateTxn;
        [SerializeField] TextAsset checkWordScript;
        [SerializeField] TextAsset submitGuessTxn;

        // Cadence scripts to get the data needed to display the High Scores panel
        [Header("Highscore Scripts")]
        [SerializeField] TextAsset GetHighScores;
        [SerializeField] TextAsset GetPlayerCumulativeScore;
        [SerializeField] TextAsset GetPlayerWinningStreak;
        [SerializeField] TextAsset GetPlayerMaxWinningStreak;
        [SerializeField] TextAsset GetGuessDistribution;

        private static FlowInterface m_instance = null;
        public static FlowInterface Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<FlowInterface>();
                }

                return m_instance;
            }
        }

        private void Start()
        {
            if (Instance != this)
            {
                Destroy(this);
            }

            // Register DevWallet

        }

        /// <summary>
        /// Attempts to log in by executing a transaction using the provided credentials
        /// </summary>
        /// <param name="username">An arbitrary username the player would like to be known by on the leaderboards</param>
        /// <param name="onSuccessCallback">Function that should be called when login is successful</param>
        /// <param name="onFailureCallback">Function that should be called when login fails</param>
        public void Login(string username, System.Action<string, string> onSuccessCallback, System.Action onFailureCallback)
        {
            // Authenticate an account with DevWallet

        }

        /// <summary>
        /// Success callback for Wallet Provider's Authenticate method. 
        /// </summary>
        /// <param name="username">The name that the user has provided (for leaderboard)</param>
        /// <param name="flowAddress">The address of the authenticated Flow Account</param>
        /// <param name="onSuccessCallback">Game callback for successful login</param>
        /// <param name="onFailureCallback">Game callback for failed login</param>
        /// <returns></returns>
        private IEnumerator OnAuthSuccess(string username, string flowAddress, System.Action<string, string> onSuccessCallback, System.Action onFailureCallback)
        {
            // get flow account from address

            // execute log in transaction on chain

            // check for error. if there was an error, break.

            // login successful!

            yield return null;
        }

        /// <summary>
        /// Clear the FLOW account object
        /// </summary>
        public void Logout()
        {
        }

        /// <summary>
        /// Attempts to get the current game state for the user from chain.
        /// </summary>
        /// <param name="onSuccessCallback">Callback on success</param>
        /// <param name="onFailureCallback">Callback on failure</param>
        public IEnumerator GetGameDataFromChain(System.Action<double, List<GuessResult>, Dictionary<string, string>> onSuccessCallback, System.Action onFailureCallback)
        {
            // execute getCurrentGameState transaction on chain

            // check for error. if so, break.

            // transaction success, get data from emitted events

            // process currentGameState event

            // process gameStartTime event

            // call GameManager to set game state

            yield return null;
        }

        /// <summary>
        /// Attemps to submit a guess for the current active game.
        /// </summary>
        /// <param name="word">The word to guess</param>
        /// <param name="onSuccessCallback">Callback on success</param>
        /// <param name="onFailureCallback">Callback on failure</param>
        /// <returns></returns>
        public IEnumerator SubmitGuess(string word, System.Action<string, string> onSuccessCallback, System.Action onFailureCallback)
        {
            // submit word via checkWord script to FLOW chain to check if word is valid

            // if word is valid, submit guess via transaction to FLOW chain

            // get wordscore

            // process result

            yield return null;
        }

        /// <summary>
        /// Loads the Highscore data from chain. Global leaderboard, and detailed stats for calling account.
        /// </summary>
        /// <param name="onSuccessCallback">Callback on success</param>
        /// <param name="onFailureCallback">Callback on failure</param>
        public IEnumerator LoadHighScoresFromChain(System.Action<List<ScoreStruct>, uint, uint, uint, uint[]> onSuccessCallback, System.Action onFailureCallback)
        {
            // execute scripts to get highscore data

            // wait for completion

            // check for errors

            // load global highscores

            // load player scores

            // callback

            yield return null;
        }
    }
}
