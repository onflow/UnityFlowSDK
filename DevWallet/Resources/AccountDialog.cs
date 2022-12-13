using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DapperLabs.Flow.Sdk.Unity;

namespace DapperLabs.Flow.Sdk.DevWallet
{
    public class AccountDialog : MonoBehaviour
    {
        [Header("UI Hookups")]
        [SerializeField] Dropdown accountDropdown;
        [SerializeField] Text gatewayText;
        [SerializeField] Button okButton;
        [SerializeField] Button cancelButton;

        List<FlowControl.Account> accounts = new List<FlowControl.Account>();

        /// <summary>
        /// Example: Init(FlowControl.GatewayCache.Values.ToArray()[0], (s) => Debug.Log(s), () => Debug.Log("Cancelled"));
        /// </summary>
        /// <param name="gateway">The gateway to use for account selection and authorization.</param>
        /// <param name="onSuccessCallback">Callback on account auth success.</param>
        /// <param name="onFailureCallback">Callback on account auth failure.</param>
        public void Init(Gateway gateway, Action<string> onSuccessCallback, Action onFailureCallback)
        {
            // set gateway text
            gatewayText.text = $"Gateway: {gateway.Name}";

            // register buttons
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(() => onSuccessCallback(accounts[accountDropdown.value].AccountConfig["Address"]));
            okButton.onClick.AddListener(() => Destroy(this.gameObject));

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => onFailureCallback());
            cancelButton.onClick.AddListener(() => Destroy(this.gameObject));

            // get list of accounts
            accounts.Clear();
            List<string> accountNames = new List<string>();
            foreach (FlowControl.Account account in FlowControl.Data.Accounts)
            {
                if (account.GatewayName == gateway.Name)
                {
                    accounts.Add(account);
                    accountNames.Add(account.Name);
                }
            }

            accountDropdown.ClearOptions();

            if (accounts.Count > 0)
            {
                // populate dropdown
                accountDropdown.AddOptions(accountNames);
            }
            else
            {
                // disable OK button
                okButton.interactable = false;
                accountDropdown.interactable = false;
                accountDropdown.AddOptions(new List<string>() { "No accounts available" });
            }
        }
    }
}
