using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FlowWordsTutorial
{
    /// <summary>
    /// Represents a row of letters
    /// </summary>
    public class UIGameBoardRow : MonoBehaviour
    {
        [SerializeField] Image[] m_cellBackgrounds;
        [SerializeField] TextMeshProUGUI[] m_cellText;

        /// <summary>
        /// Sets the text that the boxes in the row should contain
        /// </summary>
        /// <param name="text">The string that should be displayed in the row's boxes</param>
        public void SetRowText(string text)
        {
            for (int cell = 0; cell < 5; cell++)
            {
                string cellText = text.Length > cell ? text[cell].ToString() : "";
                m_cellText[cell].text = cellText;
            }
        }

        /// <summary>
        /// Sets the background color of each box in a row.
        /// </summary>
        /// <param name="colours">A string representing the colors</param>
        public void SetRowColours(string colours)
        {
            for (int cell = 0; cell < 5; cell++)
            {
                Material cellMaterial = UIManager.Instance.Default;

                switch (colours[cell])
                {
                    case 'p':
                        cellMaterial = UIManager.Instance.LetterRightPlace;
                        break;
                    case 'w':
                        cellMaterial = UIManager.Instance.LetterWrongPlace;
                        break;
                    case 'n':
                        cellMaterial = UIManager.Instance.LetterNotUsed;
                        break;
                }

                m_cellBackgrounds[cell].material = cellMaterial;
                m_cellBackgrounds[cell].SetMaterialDirty();
            }
        }
    }
}
