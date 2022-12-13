using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace FlowWordsTutorial
{
    /// <summary>
    /// Controls the main gameplay interface.
    /// </summary>
    public class GamePanel : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI m_countdownTimer;
        [SerializeField] TextMeshProUGUI m_status;
        [SerializeField] TextMeshProUGUI wonLostMessage;
        [SerializeField] UIGameBoard m_gameBoard;
        [SerializeField] UIKeyboard m_keyboard;

        private string m_currentEntry = "";

        void Update()
        {
            UpdateTimer();
            PollKeyboard();
        }

        /// <summary>
        /// Updates the displayed time to next word timer.
        /// </summary>
        private void UpdateTimer()
        {
            if (m_countdownTimer != null)
            {
                DateTime unixTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                unixTime = unixTime.AddSeconds(GameManager.Instance.CurrentGameStartTime).ToLocalTime();

                TimeSpan difference = TimeSpan.FromHours(24.0) - (DateTime.Now - unixTime);
                if (difference.TotalSeconds < 0)
                {
                    difference = new TimeSpan(0, 0, 0);
                }

                string timeToNext = $"{difference.Hours.ToString("00")}:{difference.Minutes.ToString("00")}:{difference.Seconds.ToString("00")}";
                m_countdownTimer.text = $"Time to next game: {timeToNext}";
            }
        }

        /// <summary>
        /// Poll for physical keyboard input.
        /// </summary>
        private void PollKeyboard()
        {
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    ProcessKey(KeyCode.Backspace);
                }

                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    ProcessKey(KeyCode.Return);
                }

                // check over the acceptable keys
                for (int i = 0; i < 26; i++)
                {
                    KeyCode key = (KeyCode.A + i);

                    if (Input.GetKeyDown(key))
                    {
                        ProcessKey(key);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Process a physical or virtual key press
        /// </summary>
        /// <param name="keyCode">The KeyCode of the key that was pressed</param>
        private void ProcessKey(KeyCode keyCode)
        {
            if (GameManager.Instance.CurrentGameState != GameState.PLAYING)
            {
                return;
            }

            if (keyCode == KeyCode.Backspace)
            {
                if (m_currentEntry.Length > 0)
                {
                    m_currentEntry = m_currentEntry.Substring(0, m_currentEntry.Length - 1);
                    m_gameBoard.UpdateCurrentEntry(m_currentEntry);
                }
                return;
            }

            if (keyCode == KeyCode.KeypadEnter || keyCode == KeyCode.Return)
            {
                if (m_currentEntry.Length == 5)
                {
                    GameManager.Instance.SubmitGuess(m_currentEntry);
                }
                return;
            }

            if (m_currentEntry.Length == 5)
            {
                return;
            }

            if ("QWERTYUIOPASDFGHJKLZXCVBNM".Contains(keyCode.ToString()))
            {
                m_currentEntry += keyCode.ToString().ToUpper();
                m_gameBoard.UpdateCurrentEntry(m_currentEntry);
            }
        }

        /// <summary>
        /// Handle the press of a key on the virtual onscreen keyboard
        /// </summary>
        /// <param name="button">The string representation of the pressed key</param>
        public void VirtualKeyboardButtonPressed(string button)
        {
            switch (button.ToUpper())
            {
                case "SUBMIT":
                    ProcessKey(KeyCode.Return);
                    break;
                case "BACKSPACE":
                    ProcessKey(KeyCode.Backspace);
                    break;
                default:
                    KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), button);
                    ProcessKey(key);
                    break;
            }
        }

        /// <summary>
        /// Set the status text
        /// </summary>
        /// <param name="status">The text that should be displayed in the status area</param>
        internal void SetStatus(string status)
        {
            m_status.text = status;
        }

        /// <summary>
        /// Updates the game interface to reflect current game status
        /// </summary>
        /// <param name="results">An array of GuessResult.</param>
        /// <param name="keyStatuses">A Dictionary mapping keys to their status</param>
        public void UpdateGameInterface(GuessResult[] results, Dictionary<string, string> keyStatuses)
        {

            // update gameBoard rows
            m_gameBoard.UpdateRows(results);

            // update current entry
            m_gameBoard.UpdateCurrentEntry(m_currentEntry);

            // update keyboard
            m_keyboard.UpdateKeys(keyStatuses);

            //hide keyboard and display message if won or lost.
            if (GameManager.Instance.CurrentGameState == GameState.WON || GameManager.Instance.CurrentGameState == GameState.LOST)
            {
                //Hide virtual keyboard
                UIKeyboard keyboard = GameObject.FindObjectOfType<UIKeyboard>();
                if (keyboard != null)
                {
                    keyboard.gameObject.SetActive(false);
                }

                //Display won/lost message as appropriate
                wonLostMessage.text = GameManager.Instance.CurrentGameState == GameState.WON ? "You win!" : "Out of guesses!";
            }
            else
            {
                wonLostMessage.text = "";
            }
        }

        /// <summary>
        /// Clears the current guess row.
        /// </summary>
        internal void ClearCurrentEntry()
        {
            m_currentEntry = "";
            m_gameBoard.UpdateCurrentEntry(m_currentEntry);
        }

        /// <summary>
        /// Displays the High Scores panel when the High Scores button is clicked.
        /// </summary>
        public void ShowHighScoresClicked()
        {
            GameManager.Instance.ShowHighScores();
        }

        /// <summary>
        /// Logs out the user and returns them to the login screen when Log Out button clicked.
        /// </summary>
        public void LogOutClicked()
        {
            GameManager.Instance.LogOut();
        }
    }
}
