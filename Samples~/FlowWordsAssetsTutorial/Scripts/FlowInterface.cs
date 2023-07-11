using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Unity;

namespace FlowWordsTutorial
{
    public class FlowInterface : MonoBehaviour
    {
        /// <summary>
        /// Class to hold the payload of a cadence CurrentState event
        /// </summary>
        public class StatePayload
        {
            public List<GuessResult> currentState;
        }

        /// <summary>
        /// Class to hold the payload of a cadence LastGameStart event
        /// </summary>
        public class TimePayload
        {
            public Decimal startTime;
        }

        /// <summary>
        /// Class to hold the payload of a cadence GuessResult event
        /// </summary>
        public class GuessResultPayload
        {
            public string result;
        }
        
        // The TextAssets containing Cadence scripts and transactions that will be used for the game.
        [Header("Scripts and Transactions")]
        [SerializeField] CadenceTransactionAsset loginTxn;
        [SerializeField] CadenceScriptAsset checkWordScript;
        [SerializeField] CadenceTransactionAsset submitGuessTxn;

        // Cadence scripts to get the data needed to display the High Scores panel
        [Header("Highscore Scripts")]
        [SerializeField] CadenceScriptAsset GetHighScores;
        [SerializeField] CadenceScriptAsset GetPlayerCumulativeScore;
        [SerializeField] CadenceScriptAsset GetPlayerWinningStreak;
        [SerializeField] CadenceScriptAsset GetPlayerMaxWinningStreak;
        [SerializeField] CadenceScriptAsset GetGuessDistribution;

        // FlowControl Account object, used to help with text replacements in scripts and transactions
        //private FlowControl.Account FLOW_ACCOUNT = null;

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
        /// Clear the FLOW account object
        /// </summary>
        public void Logout()
        {
        }

        /// <summary>
        /// Attempts to get the current game state for the user from chain.
        /// </summary>
        /// <param name="username">An arbitrary username the player would like to be known by on the leaderboards</param>
        /// <param name="onSuccessCallback">Callback on success</param>
        /// <param name="onFailureCallback">Callback on failure</param>
        public IEnumerator GetGameDataFromChain(string username, System.Action<Decimal, List<GuessResult>, Dictionary<string, string>> onSuccessCallback, System.Action onFailureCallback)
        {
            // get FLOW_ACCOUNT object for text replacements

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
        public IEnumerator LoadHighScoresFromChain(System.Action<List<ScoreStruct>, BigInteger, BigInteger, BigInteger, List<BigInteger>> onSuccessCallback, System.Action onFailureCallback)
        {
            // get player's wallet public address

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
