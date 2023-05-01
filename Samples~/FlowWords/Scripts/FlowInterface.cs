using System;
using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.Crypto;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Unity;
using DapperLabs.Flow.Sdk.WalletConnect;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using UnityEngine;
using Convert = DapperLabs.Flow.Sdk.Cadence.Convert;

namespace FlowWords
{
    public class FlowInterface : MonoBehaviour
    {
        // The TextAssets containing Cadence scripts and transactions that will be used for the game.
        [Header("Scripts and Transactions")]
        [SerializeField] CadenceTransactionAsset loginTxn;
        [SerializeField] CadenceTransactionAsset getCurrentGameStateTxn;
        [SerializeField] CadenceScriptAsset checkWordScript;
        [SerializeField] CadenceTransactionAsset submitGuessTxn;

        // Cadence scripts to get the data needed to display the High Scores panel
        [Header("Highscore Scripts")]
        [SerializeField] CadenceScriptAsset GetHighScores;
        [SerializeField] CadenceScriptAsset GetPlayerCumulativeScore;
        [SerializeField] CadenceScriptAsset GetPlayerWinningStreak;
        [SerializeField] CadenceScriptAsset GetPlayerMaxWinningStreak;
        [SerializeField] CadenceScriptAsset GetGuessDistribution;

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

            // Set up SDK to access TestNet
            FlowConfig flowConfig = new FlowConfig()
            {
                NetworkUrl = "https://rest-testnet.onflow.org/v1",  // testnet
                Protocol = FlowConfig.NetworkProtocol.HTTP
            };
            FlowSDK.Init(flowConfig);

            // Create WalletConnect wallet provider
            IWallet walletProvider = new WalletConnectProvider();
            walletProvider.Init(new WalletConnectConfig
            {
                ProjectId = "fb087df84af28bc20669151a5efb3ff7", // insert Project ID from Wallet Connect dashboard
                ProjectDescription = "A simple word guessing game built on FLOW!",
                ProjectIconUrl = "https://walletconnect.com/meta/favicon.ico",
                ProjectName = "FlowWords",
                ProjectUrl = "https://dapperlabs.com"
            });

            // Register WalletConnect wallet provider with SDK
            FlowSDK.RegisterWalletProvider(walletProvider);
        }

        private string DoTextSubstitutions(string input)
        {
            FlowControl.Account fa = new FlowControl.Account
            {
                GatewayName = "Flow Testnet",
                AccountConfig = new Dictionary<string, string> { { "Address", FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address } }
            };

            return fa.DoTextReplacements(input);
        }

        /// <summary>
        /// Attempts to log in by executing a transaction using the provided credentials
        /// </summary>
        /// <param name="username">An arbitrary username the player would like to be known by on the leaderboards</param>
        /// <param name="onSuccessCallback">Function that should be called when login is successful</param>
        /// <param name="onFailureCallback">Function that should be called when login fails</param>
        public void Login(string username, System.Action<string, string> onSuccessCallback, System.Action onFailureCallback)
        {
            // Authenticate an account
            if (FlowSDK.GetWalletProvider().IsAuthenticated() == false)
            {
                FlowSDK.GetWalletProvider().Authenticate("", // blank string will show list of accounts from Accounts tab of Flow Control Window
                                                        (string address) => onSuccessCallback(address, username), 
                                                        onFailureCallback);
            }
        }


        /// <summary>
        /// Clear the FLOW account object
        /// </summary>
        internal void Logout()
        {
            FlowSDK.GetWalletProvider().Unauthenticate();
        }

        
        public class StatePayload
        {
            public List<GuessResult> currentState;
        }

        public class TimePayload
        {
            public Decimal startTime;
        }

        public class GuessResultPayload
        {
            public string result;
        }
        
        /// <summary>
        /// Attempts to get the current game state for the user from chain.
        /// </summary>
        /// <param name="onSuccessCallback">Callback on success</param>
        /// <param name="onFailureCallback">Callback on failure</param>
        public IEnumerator GetGameDataFromChain(string username, System.Action<Decimal, List<GuessResult>, Dictionary<string, string>> onSuccessCallback, System.Action onFailureCallback)
        {
            // execute getCurrentGameState transaction on chain
            string txnId = null;
            Task<FlowTransactionResult> getStateTask = Transactions.SubmitAndWaitUntilExecuted((string id) => { txnId = id; }, DoTextSubstitutions(loginTxn.text), new CadenceString(username));

            while (!getStateTask.IsCompleted)
            {
                int dots = ((int)(Time.time * 2.0f) % 4);

                if (txnId == null)
                {
                    UIManager.Instance.SetStatus("Waiting for user to sign transaction" + new string('.', dots));
                }
                else
                {
                    UIManager.Instance.SetStatus($"Retrieving data from chain" + new string('.', dots));
                }

                yield return null;
            }

            // check for error. if so, break.
            if (getStateTask.Result.Error != null || getStateTask.Result.ErrorMessage != string.Empty || getStateTask.Result.Status == FlowTransactionStatus.EXPIRED)
            {
                onFailureCallback();
                yield break;
            }

            // get events
            List<FlowEvent> events = getStateTask.Result.Events;
            FlowEvent currentStateEvent = events.Find(x => x.Type.EndsWith(".CurrentState"));
            FlowEvent startTimeEvent = events.Find(x => x.Type.EndsWith(".LastGameStart"));

            if (currentStateEvent == null || startTimeEvent == null)
            {
                onFailureCallback();
                yield break;
            }

            // transaction success, get data from emitted events
            Decimal gameStartTime = 0;
            Dictionary<string, string> letterStatuses = new Dictionary<string, string>();
            
            // process current game state event
            List<GuessResult> results = Convert.FromCadence<StatePayload>(currentStateEvent.Payload).currentState;
            foreach (GuessResult newResult in results)
            {
                newResult.word = newResult.word.ToUpper();
                for (int i = 0; i < 5; i++)
                {
                    bool letterAlreadyExists = letterStatuses.ContainsKey(newResult.word[i].ToString());
                    string currentStatus = letterAlreadyExists ? letterStatuses[newResult.word[i].ToString()] : "";
                    switch (currentStatus)
                    {
                        case "":
                            letterStatuses[newResult.word[i].ToString()] = newResult.colorMap[i].ToString();
                            break;
                        case "p":
                            break;
                        case "w":
                            if (newResult.colorMap[i] == 'p')
                            {
                                letterStatuses[newResult.word[i].ToString()] = newResult.colorMap[i].ToString();
                            }

                            break;
                        case "n":
                            if (newResult.colorMap[i] == 'p' || newResult.colorMap[i] == 'w')
                            {
                                letterStatuses[newResult.word[i].ToString()] = newResult.colorMap[i].ToString();
                            }

                            break;
                    }
                }
            }

            // get game start time event
            gameStartTime = Convert.FromCadence<TimePayload>(startTimeEvent.Payload).startTime;
            
            // call GameManager to set game state
            onSuccessCallback(gameStartTime, results, letterStatuses);
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
            Task<FlowScriptResponse> checkWordTask = Scripts.ExecuteAtLatestBlock(DoTextSubstitutions(checkWordScript.text), new CadenceString(word.ToLower()));

            while (!checkWordTask.IsCompleted)
            {
                int dots = ((int)(Time.time * 2.0f) % 4);
                UIManager.Instance.SetStatus("Waiting for server" + new string('.', dots));
                yield return null;
            }

            if (checkWordTask.Result.Error != null)
            {
                onFailureCallback();
                UIManager.Instance.SetStatus("Error checking word validity.");
                yield break;
            }

            bool wordValid = ((checkWordTask.Result.Value as CadenceString).Value == "OK");
            if (wordValid == false)
            {
                onFailureCallback();
                yield break;
            }

            // word is valid, submit guess via transaction to FLOW chain
            string txnId = null;
            Task<FlowTransactionResult> submitGuessTask = Transactions.SubmitAndWaitUntilExecuted((string id) => { txnId = id; }, DoTextSubstitutions(submitGuessTxn.text), new CadenceString(word.ToLower()));

            while (!submitGuessTask.IsCompleted)
            {
                int dots = ((int)(Time.time * 2.0f) % 4);

                if (txnId == null)
                {
                    UIManager.Instance.SetStatus("Waiting for user to sign transaction" + new string('.', dots));
                }
                else
                {
                    UIManager.Instance.SetStatus("Retrieving data from chain" + new string('.', dots));
                }

                yield return null;
            }

            if (submitGuessTask.Result.Error != null || submitGuessTask.Result.ErrorMessage != string.Empty || submitGuessTask.Result.Status == FlowTransactionStatus.EXPIRED)
            {
                onFailureCallback();
                yield break;
            }

            // get wordscore
            string wordScore = "";
            FlowEvent ourEvent = submitGuessTask.Result.Events.Find(x => x.Type.EndsWith(".GuessResult"));
            if (ourEvent != null)
            {
                wordScore = Convert.FromCadence<GuessResultPayload>(ourEvent.Payload).result;

                // check if we are out of guesses
                if (wordScore == "OutOfGuesses")
                {
                    onFailureCallback();
                    UIManager.Instance.SetStatus("Out Of Guesses. Try again tomorrow.");
                    yield break;
                }

                // process result
                onSuccessCallback(word, wordScore);
            }
            else
            {
                onFailureCallback();
            }

        }

        /// <summary>
        /// Loads the Highscore data from chain. Global leaderboard, and detailed stats for calling account.
        /// </summary>
        /// <param name="onSuccessCallback">Callback on success</param>
        /// <param name="onFailureCallback">Callback on failure</param>
        public IEnumerator LoadHighScoresFromChain(System.Action<List<ScoreStruct>, BigInteger, BigInteger, BigInteger, List<BigInteger>> onSuccessCallback, System.Action onFailureCallback)
        {
            string playerWalletAddress = FlowSDK.GetWalletProvider().GetAuthenticatedAccount().Address;

            // execute scripts to get highscore data
            Dictionary<string, Task<FlowScriptResponse>> tasks = new Dictionary<string, Task<FlowScriptResponse>>();
            tasks.Add("GetHighScores", Scripts.ExecuteAtLatestBlock(DoTextSubstitutions(GetHighScores.text)));
            tasks.Add("GetPlayerCumulativeScore", Scripts.ExecuteAtLatestBlock(DoTextSubstitutions(GetPlayerCumulativeScore.text), new CadenceAddress(playerWalletAddress)));
            tasks.Add("GetPlayerWinningStreak", Scripts.ExecuteAtLatestBlock(DoTextSubstitutions(GetPlayerWinningStreak.text), new CadenceAddress(playerWalletAddress)));
            tasks.Add("GetPlayerMaxWinningStreak", Scripts.ExecuteAtLatestBlock(DoTextSubstitutions(GetPlayerMaxWinningStreak.text), new CadenceAddress(playerWalletAddress)));
            tasks.Add("GetGuessDistribution", Scripts.ExecuteAtLatestBlock(DoTextSubstitutions(GetGuessDistribution.text), new CadenceAddress(playerWalletAddress)));

            // wait for completion
            bool complete = false;
            while (!complete)
            {
                complete = true;
                foreach (KeyValuePair<string, Task<FlowScriptResponse>> task in tasks)
                {
                    complete = complete && task.Value.IsCompleted;
                }
                yield return null;
            }

            // check for errors
            foreach (KeyValuePair<string, Task<FlowScriptResponse>> task in tasks)
            {
                if (task.Value.Result.Error != null)
                {
                    onFailureCallback();
                    yield break;
                }
            }

            // load global highscores
            List<ScoreStruct> GlobalScores = Convert.FromCadence<List<ScoreStruct>>(tasks["GetHighScores"].Result.Value);
            GlobalScores = GlobalScores.OrderByDescending(score => score.Score).Take(10).ToList();

            // load player scores
            BigInteger PlayerCumulativeScore = Convert.FromCadence<BigInteger>(tasks["GetPlayerCumulativeScore"].Result.Value);
            BigInteger PlayerWinningStreak = Convert.FromCadence<BigInteger>(tasks["GetPlayerWinningStreak"].Result.Value);
            BigInteger PlayerMaximumWinningStreak = Convert.FromCadence<BigInteger>(tasks["GetPlayerMaxWinningStreak"].Result.Value);
            List<BigInteger> PlayerGuessDistribution = Convert.FromCadence<List<BigInteger>>(tasks["GetGuessDistribution"].Result.Value);

            // callback
            onSuccessCallback(GlobalScores, PlayerCumulativeScore, PlayerWinningStreak, PlayerMaximumWinningStreak, PlayerGuessDistribution);
        }
    }
}
