using UnityEngine;

namespace FlowWords
{
    /// <summary>
    /// Represents the Login panel.
    /// </summary>
    public class LoginPanel : MonoBehaviour
    {
        // user information
        private string Username = "";

        // UI hookups
        [SerializeField] TMPro.TMP_InputField UsernameTMP;
        [SerializeField] TMPro.TextMeshProUGUI Status;

        /// <summary>
        /// When enabled, load data from last play from PlayerPrefs and sets the text box contents accordingly
        /// </summary>
        void OnEnable()
        {
            // populate from PlayerPrefs
            Username = PlayerPrefs.GetString("Username", "");

            UsernameTMP.text = Username;

            Status.text = "";
        }

        /// <summary>
        /// Handler for clicking of the "Log In" button
        /// </summary>
        public void LoginClicked()
        {
            Username = UsernameTMP.text;

            PlayerPrefs.SetString("Username", Username);
            PlayerPrefs.Save();

            GameManager.Instance.Login(Username);
        }

        /// <summary>
        /// Sets the status text of the Login panel
        /// </summary>
        /// <param name="status">String that should be displayed</param>
        internal void SetStatus(string status)
        {
            Status.text = status;
        }
    }
}
