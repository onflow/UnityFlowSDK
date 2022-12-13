using DapperLabs.Flow.Sdk;
using DapperLabs.Flow.Sdk.Cadence;
using DapperLabs.Flow.Sdk.DataObjects;
using DapperLabs.Flow.Sdk.Unity;
using DapperLabs.Flow.Sdk.DevWallet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace FlowWords
{
    public class FlowInterface : MonoBehaviour
    {
        // FLOW account object - set via Login screen.
        [Header("FLOW Account")]
        public FlowControl.Account FLOW_ACCOUNT = null;

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
            FlowSDK.RegisterWalletProvider(ScriptableObject.CreateInstance<DevWalletProvider>());
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
            FlowSDK.GetWalletProvider().Authenticate("", // blank string will show list of accounts from Accounts tab of Flow Control Window
                                                    (string flowAddress) => StartCoroutine(OnAuthSuccess(username, flowAddress, onSuccessCallback, onFailureCallback)), 
                                                    onFailureCallback);
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
            // get flow account
            FLOW_ACCOUNT = FlowControl.Data.Accounts.FirstOrDefault(x => x.AccountConfig["Address"] == flowAddress);
            
            // execute log in transaction on chain
            Task<FlowTransactionResult> task = FLOW_ACCOUNT.SubmitAndWaitUntilExecuted(loginTxn.text, new CadenceString(username));
            while (!task.IsCompleted)
            {
                int dots = ((int)(Time.time * 2.0f) % 4);
                UIManager.Instance.SetStatus("Connecting" + new string('.', dots));
                yield return null;
            }

            // check for error. if there was an error, break.
            if (task.Result.Error != null || task.Result.ErrorMessage != string.Empty || task.Result.Status == FlowTransactionStatus.EXPIRED)
            {
                onFailureCallback();
                yield break;
            }

            // login successful!
            onSuccessCallback(username, flowAddress);
        }

        /// <summary>
        /// Clear the FLOW account object
        /// </summary>
        internal void Logout()
        {
            FLOW_ACCOUNT = null;
            FlowSDK.GetWalletProvider().Unauthenticate();
        }

        /// <summary>
        /// Attempts to get the current game state for the user from chain.
        /// </summary>
        /// <param name="onSuccessCallback">Callback on success</param>
        /// <param name="onFailureCallback">Callback on failure</param>
        public IEnumerator GetGameDataFromChain(System.Action<double, List<GuessResult>, Dictionary<string, string>> onSuccessCallback, System.Action onFailureCallback)
        {
            // execute getCurrentGameState transaction on chain
            Task<FlowTransactionResult> getStateTask = FLOW_ACCOUNT.SubmitAndWaitUntilExecuted(getCurrentGameStateTxn.text);
            while (!getStateTask.IsCompleted)
            {
                int dots = ((int)(Time.time * 2.0f) % 4);
                UIManager.Instance.SetStatus("Loading" + new string('.', dots));
                yield return null;
            }

            // check for error. if so, break.
            if (getStateTask.Result.Error != null || getStateTask.Result.ErrorMessage != string.Empty || getStateTask.Result.Status == FlowTransactionStatus.EXPIRED)
            {
                onFailureCallback();
                yield break;
            }

            // transaction success, get data from emitted events
            double gameStartTime = 0;
            List<GuessResult> results = new List<GuessResult>();
            Dictionary<string, string> letterStatuses = new Dictionary<string, string>();

            // get events
            List<FlowEvent> events = getStateTask.Result.Events;
            FlowEvent currentStateEvent = events.Find(x => x.Type.EndsWith(".CurrentState"));
            FlowEvent startTimeEvent = events.Find(x => x.Type.EndsWith(".LastGameStart"));

            if (currentStateEvent == null || startTimeEvent == null)
            {
                onFailureCallback();
                yield break;
            }

            // process current game state event
            // access event payload
            CadenceComposite statePayload = currentStateEvent.Payload as CadenceComposite;
            CadenceBase[] priorGuessResults = statePayload.CompositeFieldAs<CadenceArray>("currentState").Value;

            // iterate over prior guess results in payload, add to results list, and populate letter statuses
            foreach (CadenceBase result in priorGuessResults)
            {
                // add guess result to output
                GuessResult newResult = new GuessResult
                {
                    word = (result as CadenceComposite).CompositeFieldAs<CadenceString>("Guess").Value.ToUpper(),
                    colorMap = (result as CadenceComposite).CompositeFieldAs<CadenceString>("Result").Value
                };
                results.Add(newResult);

                // add letters to lettermap
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
            CadenceComposite timePayload = startTimeEvent.Payload as CadenceComposite;
            CadenceNumber startTime = timePayload.CompositeFieldAs<CadenceNumber>("startTime");
            gameStartTime = double.Parse(startTime.Value);

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
            Task<FlowScriptResponse> checkWordTask = FLOW_ACCOUNT.ExecuteScript(checkWordScript.text, new CadenceString(word.ToLower()));

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
            Task<FlowTransactionResult> submitGuessTask = FLOW_ACCOUNT.SubmitAndWaitUntilExecuted(submitGuessTxn.text, new CadenceString(word.ToLower()));

            while (!submitGuessTask.IsCompleted)
            {
                int dots = ((int)(Time.time * 2.0f) % 4);
                UIManager.Instance.SetStatus("Waiting for server" + new string('.', dots));
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
                CadenceComposite payload = ourEvent.Payload as CadenceComposite;
                wordScore = payload.CompositeFieldAs<CadenceString>("result").Value;

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
        public IEnumerator LoadHighScoresFromChain(System.Action<List<ScoreStruct>, uint, uint, uint, uint[]> onSuccessCallback, System.Action onFailureCallback)
        {
            // execute scripts to get highscore data
            Dictionary<string, Task<FlowScriptResponse>> tasks = new Dictionary<string, Task<FlowScriptResponse>>();
            tasks.Add("GetHighScores", FLOW_ACCOUNT.ExecuteScript(GetHighScores.text));
            tasks.Add("GetPlayerCumulativeScore", FLOW_ACCOUNT.ExecuteScript(GetPlayerCumulativeScore.text, new CadenceAddress(FLOW_ACCOUNT.AccountConfig["Address"])));
            tasks.Add("GetPlayerWinningStreak", FLOW_ACCOUNT.ExecuteScript(GetPlayerWinningStreak.text, new CadenceAddress(FLOW_ACCOUNT.AccountConfig["Address"])));
            tasks.Add("GetPlayerMaxWinningStreak", FLOW_ACCOUNT.ExecuteScript(GetPlayerMaxWinningStreak.text, new CadenceAddress(FLOW_ACCOUNT.AccountConfig["Address"])));
            tasks.Add("GetGuessDistribution", FLOW_ACCOUNT.ExecuteScript(GetGuessDistribution.text, new CadenceAddress(FLOW_ACCOUNT.AccountConfig["Address"])));

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
            CadenceBase[] highscores = (tasks["GetHighScores"].Result.Value as CadenceArray).Value;
            List<ScoreStruct> GlobalScores = new List<ScoreStruct>();
            foreach (CadenceComposite score in highscores)
            {
                ScoreStruct parsedScore = new ScoreStruct("", 0);
                foreach (CadenceCompositeField field in score.Value.Fields)
                {
                    switch (field.Name)
                    {
                        case "Name":
                            parsedScore.Name = (field.Value as CadenceString).Value;
                            break;
                        case "Score":
                            parsedScore.Score = int.Parse((field.Value as CadenceNumber).Value);
                            break;
                    }
                }

                GlobalScores.Add(parsedScore);
            }
            GlobalScores = GlobalScores.OrderByDescending(score => score.Score).Take(10).ToList();

            // load player scores
            uint PlayerCumulativeScore = uint.Parse((tasks["GetPlayerCumulativeScore"].Result.Value as CadenceNumber).Value);
            uint PlayerWinningStreak = uint.Parse((tasks["GetPlayerWinningStreak"].Result.Value as CadenceNumber).Value);
            uint PlayerMaximumWinningStreak = uint.Parse((tasks["GetPlayerMaxWinningStreak"].Result.Value as CadenceNumber).Value);
            uint[] PlayerGuessDistribution = (tasks["GetGuessDistribution"].Result.Value as CadenceArray).Value.Select(value => uint.Parse((value as CadenceNumber).Value)).ToArray();

            // callback
            onSuccessCallback(GlobalScores, PlayerCumulativeScore, PlayerWinningStreak, PlayerMaximumWinningStreak, PlayerGuessDistribution);
        }
    }
}
