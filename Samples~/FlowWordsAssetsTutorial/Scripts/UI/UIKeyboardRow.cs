using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FlowWordsTutorial
{
    /// <summary>
    /// Represents a row of letters on the virtual keyboard
    /// </summary>
    public class UIKeyboardRow : MonoBehaviour
    {
        [SerializeField] string m_letters;
        [SerializeField] Button[] m_buttons;

        /// <summary>
        /// Sets up all the buttons in the row with the proper text upon Awake
        /// </summary>
        void Awake()
        {
            // loop over buttons and register them
            for (int button = 0; button < m_buttons.Length; button++)
            {
                // get button label
                string buttonLabel;
                switch (m_letters[button])
                {
                    case '@':
                        buttonLabel = "Submit";
                        break;
                    case '^':
                        buttonLabel = "Backspace";
                        break;
                    default:
                        buttonLabel = m_letters[button].ToString();
                        break;
                }

                // apply text to tmp object
                TMPro.TextMeshProUGUI buttonText = m_buttons[button].GetComponentInChildren<TMPro.TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = buttonLabel;
                }

                // link button to GameManager callback        
                m_buttons[button].onClick.AddListener(() => UIManager.Instance.VirtualKeyboardButtonPressed(buttonLabel));
            }
        }

        public void UpdateKeyStatuses(Dictionary<string, string> keyStatuses)
        {
            for (int keyIdx = 0; keyIdx < m_letters.Length; keyIdx++)
            {
                string key = m_letters[keyIdx].ToString();
                if (keyStatuses.ContainsKey(key))
                {
                    ColorBlock keyColours = m_buttons[keyIdx].colors;

                    switch (keyStatuses[key])
                    {
                        case "p":
                            keyColours.normalColor = Color.green;
                            break;
                        case "w":
                            keyColours.normalColor = Color.yellow;
                            break;
                        case "n":
                            keyColours.normalColor = Color.grey;
                            break;
                    }

                    m_buttons[keyIdx].colors = keyColours;
                }
            }
        }
    }
}
