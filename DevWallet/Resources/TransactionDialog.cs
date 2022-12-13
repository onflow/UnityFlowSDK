using System;
using UnityEngine;
using UnityEngine.UI;

namespace DapperLabs.Flow.Sdk.DevWallet
{
    public class TransactionDialog : MonoBehaviour
    {
        [Header("UI Hookups")]
        [SerializeField] Text transactionScript;
        [SerializeField] Button approveButton;
        [SerializeField] Button denyButton;

        /// <summary>
        /// Example: Init(example.text, () => Debug.Log("Approved"), () => Debug.Log("Denied"));
        /// </summary>
        /// <param name="script">The Cadence transaction script to execute.</param>
        /// <param name="onSuccessCallback">Callback on transaction success.</param>
        /// <param name="onFailureCallback">Callback on transaction failure.</param>
        public void Init(string script, Action onSuccessCallback, Action onFailureCallback)
        {
            // set transaction text
            transactionScript.text = script;

            // register buttons
            approveButton.onClick.RemoveAllListeners();
            approveButton.onClick.AddListener(() => onSuccessCallback());
            approveButton.onClick.AddListener(() => Destroy(this.gameObject));

            denyButton.onClick.RemoveAllListeners();
            denyButton.onClick.AddListener(() => onFailureCallback());
            denyButton.onClick.AddListener(() => Destroy(this.gameObject));
        }
    }
}
