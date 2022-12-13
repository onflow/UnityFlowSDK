using System.Collections.Generic;
using UnityEngine;

namespace FlowWordsTutorial
{
    public class UIKeyboard : MonoBehaviour
    {
        [SerializeField] UIKeyboardRow[] m_keyboardRows;

        /// <summary>
        /// Updates the colors of each box in all the rows
        /// </summary>
        /// <param name="keyStatuses">A dictionary mapping a key to a color</param>
        internal void UpdateKeys(Dictionary<string, string> keyStatuses)
        {
            foreach (UIKeyboardRow keyboardRow in m_keyboardRows)
            {
                keyboardRow.UpdateKeyStatuses(keyStatuses);
            }
        }
    }
}
