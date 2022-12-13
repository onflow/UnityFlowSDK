using System.Collections.Generic;
using UnityEngine;

namespace FlowWordsTutorial
{
    public class GuessResult
    {
        /// <summary>
        /// the guess that was submitted
        /// </summary>
        public string word;

        /// <summary>
        /// A 5 letter code indicating the result and resulting color a cell should be. 
        /// </summary>
        /// <remarks>
        /// "p" = the letter at this position was in the word and in the correct (p)osition, color the cell green.
        /// "w" = the letter at this position was in the (w)ord, but in the incorrect position, color the cell yellow.
        /// "n" = the letter at this position was (n)ot in the word.
        /// </remarks>
        public string colorMap;
    }

    /// <summary>
    /// Enum of the possible states the game can be in.
    /// </summary>
    public enum GameState
    {
        PLAYING,
        WON,
        LOST
    }

    /// <summary>
    /// Handles overall game state and transitions between different states.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<GameManager>();
                }

                return m_instance;
            }
        }
        private static GameManager m_instance = null;

        private List<GuessResult> m_guessResults = new List<GuessResult>();
        private Dictionary<string, string> m_letterStatuses = new Dictionary<string, string>();

        public GameState CurrentGameState { get { return m_currentGameState; } }
        private GameState m_currentGameState;

        public double CurrentGameStartTime { get { return m_currentGameStartTimeUnix; } }
        private double m_currentGameStartTimeUnix;

        /// <summary>
        /// Starts the game.
        /// </summary>
        private void Start()
        {
            if (Instance != this)
            {
                Destroy(this);
                return;
            }

            //Set the starting state to be the LOGIN state.
            UIManager.Instance.SetUiState(UIState.LOGIN);
        }

        /// <summary>
        /// Starts the login coroutine
        /// </summary>
        /// <param name="username">Username the user would like to use</param>
        public void Login(string username)
        {
            FlowInterface.Instance.Login(username, OnLoginSuccess, OnLoginFailure);
        }

        /// <summary>
        /// Function called when login is successful
        /// </summary>
        /// <param name="username">The username chosen by the user</param>
        /// <param name="address">The user's Flow address</param>
        private void OnLoginSuccess(string username, string address)
        {
            UIManager.Instance.SetStatus("Login Success - Getting game state");

            NewGame();
        }

        /// <summary>
        /// Function called when login fails
        /// </summary>
        private void OnLoginFailure()
        {
            LogOut();
            UIManager.Instance.SetStatus("Login Failed.\nPlease check your credentials.");
        }

        /// <summary>
        /// Starts a coroutine that initializes a new game from data on chain.
        /// </summary>
        public void NewGame()
        {
            StartCoroutine(FlowInterface.Instance.GetGameDataFromChain(OnNewGameSuccess, OnNewGameFailure));
        }

        /// <summary>
        /// Function called when a new game started successfully
        /// </summary>
        /// <param name="gameStartTime">The time this game was started</param>
        /// <param name="guessResults">The current list of results for this game</param>
        /// <param name="letterStatuses">A dictionary mapping keys to statuses (colors)</param>
        private void OnNewGameSuccess(double gameStartTime, List<GuessResult> guessResults, Dictionary<string, string> letterStatuses)
        {
            // set game status
            m_currentGameStartTimeUnix = gameStartTime;
            m_guessResults = guessResults;
            m_letterStatuses = letterStatuses;

            // check gameState and set
            m_currentGameState = CheckForGameOver();

            // set UI and board
            UIManager.Instance.SetUiState(UIState.GAME);
            UIManager.Instance.DrawBoard(m_guessResults, m_letterStatuses);
        }

        /// <summary>
        /// Function called if starting a new game failed
        /// </summary>
        private void OnNewGameFailure()
        {
            LogOut();
            UIManager.Instance.SetStatus("Failed to get Game State from chain. Please try again.");
        }

        /// <summary>
        /// Starts a coroutine that submits a guess to be processed by the blockchain asynchronously
        /// </summary>
        /// <param name="word">A string containing the player's guess</param>
        public void SubmitGuess(string word)
        {
            StartCoroutine(FlowInterface.Instance.SubmitGuess(word, OnSubmitGuessSuccess, OnSubmitGuessFailure));
        }

        /// <summary>
        /// Function called when a guess has been successfully submitted
        /// </summary>
        /// <param name="word">The word that was submitted</param>
        /// <param name="wordScore">The score of the submitted word</param>
        private void OnSubmitGuessSuccess(string word, string wordScore)
        {
            // add result to guessResult list
            GuessResult result = new GuessResult();
            result.word = word;
            result.colorMap = wordScore;

            m_guessResults.Add(result);

            // update letterStatuses with latest guess
            for (int j = 0; j < 5; j++)
            {
                if (m_letterStatuses.ContainsKey(word[j].ToString()))
                {
                    // only update if new letterstatus is greater than current
                    switch (m_letterStatuses[word[j].ToString()])
                    {
                        case "p":
                            break;
                        case "w":
                            if (wordScore[j] == 'p')
                            {
                                m_letterStatuses[word[j].ToString()] = wordScore[j].ToString();
                            }
                            break;
                        default:
                            m_letterStatuses[word[j].ToString()] = wordScore[j].ToString();
                            break;
                    }
                }
                else
                {
                    // set letter status
                    m_letterStatuses[word[j].ToString()] = wordScore[j].ToString();
                }
            }

            // check for game over
            m_currentGameState = CheckForGameOver();

            // clear current entry and update board
            UIManager.Instance.ClearCurrentEntry();
            UIManager.Instance.DrawBoard(m_guessResults, m_letterStatuses);
            UIManager.Instance.SetStatus("");
        }

        /// <summary>
        /// Function called when the submission of a guess fails
        /// </summary>
        private void OnSubmitGuessFailure()
        {
            UIManager.Instance.SetStatus("Invalid Entry.");
            UIManager.Instance.ClearCurrentEntry();
        }

        /// <summary>
        /// Checks the current GameState to determine if the player is in an active game or has won or lost.
        /// </summary>
        /// <returns>The current GameState</returns>
        private GameState CheckForGameOver()
        {
            //If there have been one or more guesses, and the last guess was correct, return WON state.
            if (m_guessResults.Count > 0 && m_guessResults[m_guessResults.Count - 1].colorMap == "ppppp")
            {
                return GameState.WON;
            }

            //If they have submitted 6 guesses, return the LOST state.
            if (m_guessResults.Count == 6)
            {
                return GameState.LOST;
            }

            //They still have guesses remaining and they haven't won yet, so return PLAYING state.
            return GameState.PLAYING;
        }

        /// <summary>
        /// Shows the High Scores panel
        /// </summary>
        public void ShowHighScores()
        {
            UIManager.Instance.SetUiState(UIState.HIGHSCORES);
        }

        /// <summary>
        /// Closes the High Score panel and resets the UI state if the close button is clicked.
        /// </summary>	
        public void CloseHighScores()
        {
            UIManager.Instance.SetUiState(UIState.GAME);
            UIManager.Instance.DrawBoard(m_guessResults, m_letterStatuses);
        }

        /// <summary>
        /// Logs out of the game.
        /// </summary>
        public void LogOut()
        {
            FlowInterface.Instance.Logout();
            UIManager.Instance.SetUiState(UIState.LOGIN);
        }
    }
}
