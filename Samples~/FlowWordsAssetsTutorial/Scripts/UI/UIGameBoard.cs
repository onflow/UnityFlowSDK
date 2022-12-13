using UnityEngine;

namespace FlowWordsTutorial
{
    /// <summary>
    /// Represents the game board, consisting of the rows of letter boxes.
    /// </summary>
    public class UIGameBoard : MonoBehaviour
    {
        [SerializeField] UIGameBoardRow[] m_gameBoardRows;

        private int m_entryRow = 0;

        /// <summary>
        /// Updates the rows of the game board according to the results of previous guesses.
        /// </summary>
        /// <param name="results">An array of results of previous guesses (if any)</param>
        internal void UpdateRows(GuessResult[] results)
        {
            //Process each row
            for (int row = 0; row < 6; row++)
            {
                //If there is a guess for this row, display the results.

                if (row < results.Length)
                {
                    m_gameBoardRows[row].SetRowText(results[row].word);
                    m_gameBoardRows[row].SetRowColours(results[row].colorMap);
                }
                //If there is no guess, display an empty box.
                else
                {
                    m_gameBoardRows[row].SetRowText("");
                    m_gameBoardRows[row].SetRowColours("     ");
                }
            }

            //Set the row that the next guess will be entered into.
            m_entryRow = results.Length;
        }

        /// <summary>
        /// Updates the current guess row contents based on the player input
        /// </summary>
        /// <param name="currentEntry">A string containing the guess that is currently being input</param>
        internal void UpdateCurrentEntry(string currentEntry)
        {
            if (m_entryRow < 6)
            {
                m_gameBoardRows[m_entryRow].SetRowText(currentEntry);
            }
        }
    }
}
