using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DapperLabs.Flow.Sdk.DataObjects;
using UnityEditor;
using UnityEngine;
using DapperLabs.Flow.Sdk.DevWallet;
using UnityEditor.SceneManagement;
using Debug = UnityEngine.Debug;

namespace DapperLabs.Flow.Sdk.Unity
{
    /// <summary>
    /// FlowControl editor window.  Access from Window->%Flow->%Flow Control
    /// See the <a href="md__documentation___flow_control.html">FlowControl tutorial page</a> for more information.
    /// </summary>
    public class FlowControlWindow : EditorWindow
    {
        private enum Panel
        {
            EMULATOR,
            ACCOUNTS,
            REPLACEMENT,
            TOOLS
        }

        //Used to detect script recompilation/reload.
        private static FlowControlWindow instance = null;

        private Panel currentPanel = Panel.EMULATOR;
        private Vector2 scrollPos;

        //Tools window contract variables
        private int toolsContractAccountIndex = 0;
        private CadenceContractAsset toolsContractScript = null;
        private string toolsContractName = "";
        private System.Threading.Tasks.Task<DataObjects.FlowTransactionResponse> toolsContractResponse = null;
        private System.Threading.Tasks.Task<DataObjects.FlowTransactionResult> toolsContractResult = null;
        private bool toolsContractErrorLogged;

        //Tools window transaction variables
        private int toolsTransactionAccountIndex = 0;
        private CadenceTransactionAsset toolsTransactionScript = null;
        private System.Threading.Tasks.Task<DataObjects.FlowTransactionResponse> toolsTransactionResponse = null;
        private System.Threading.Tasks.Task<DataObjects.FlowTransactionResult> toolsTransactionResult = null;
        private bool toolsTransactionErrorLogged;

        //Tools window create account variables
        private int toolsCreateAccountPayerIndex = 0;
        private string toolsCreateAccountNewAccountName = "";
        private System.Threading.Tasks.Task<SdkAccount> newAccountTask;
        private string creationStatus;

        private bool flowExecutableFound = false;
        private DateTime lastFlowCheck = DateTime.Now;


        /// <summary>
        /// Displays the FlowControl editor window.
        /// </summary>
        [MenuItem("Window/Flow/Flow Control")]
        public static void ShowFlowControl()
        {
            FlowControlWindow wnd = GetWindow<FlowControlWindow>();
            instance = wnd;
            wnd.titleContent = new GUIContent("Flow Control Manager");
        }
        
        private void CreateGUI()
        {
            //If not already set, get any existing instances.
            if (FlowControl.Instance == null)
            {
                FlowControl.Instance = FindObjectOfType<FlowControl>();

                //If we didn't find an instance, create one
                if (FlowControl.Instance == null)
                {
                    FlowControl.Instance = new GameObject("Flow Control", typeof(FlowControl)).GetComponent<FlowControl>();
                    EditorUtility.SetDirty(FlowControl.Instance);
                }
            }

            if (FlowControl.Data == null)
            {
                FlowControlData flowControlData = AssetDatabase.LoadAssetAtPath<FlowControlData>("Assets/Resources/FlowControlData.asset");

                if (flowControlData != null)
                {
                    FlowControl.Data = flowControlData;
                    EditorUtility.SetDirty(FlowControl.Data);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                else
                {
                    FlowControlData[] dataList = Resources.FindObjectsOfTypeAll<FlowControlData>();
                    if (dataList.Length > 0)
                    {
                        FlowControl.Data = dataList[0];
                    }
                    else
                    {
                        FlowControlData newFCD = CreateInstance<FlowControlData>();
                        Directory.CreateDirectory($"Assets/Resources");
                        AssetDatabase.CreateAsset(newFCD, $"Assets/Resources/FlowControlData.asset");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        FlowControl.Data = newFCD;
                    }
                    EditorUtility.SetDirty(FlowControl.Data);
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            
            CheckForFlowExecutable();

            FlowSDK.RegisterWalletProvider(new DevWalletProvider());
        }

        private void OnDestroy()
        {
            EditorUtility.SetDirty(FlowControl.Data);
            AssetDatabase.SaveAssets();
        }

        private void OnGUI()
        {
            if (FlowControl.Data == null)
            {
                CreateGUI();
            }

            //If instance is null, then window may have been reloaded due to script recompilation.  Ensure that window state is restored. 
            if (instance == null)
            {
                instance = GetWindow<FlowControlWindow>();
                CreateGUI();
            }

            bool mouseDown = Event.current.type == EventType.MouseDown;
            Vector2 mousePos = Event.current.mousePosition;
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical(GUILayout.Width(120));
                {
                    GUILayout.Space(20);

                    if (GUILayout.Button("Emulator Settings"))
                    {
                        currentPanel = Panel.EMULATOR;
                        scrollPos = new Vector2();
                        if (mouseDown && GUILayoutUtility.GetLastRect().Contains(mousePos))
                        {
                            GUI.FocusControl(null);
                        }
                    }

                    if (GUILayout.Button("Accounts"))
                    {
                        currentPanel = Panel.ACCOUNTS;
                        scrollPos = new Vector2();
                        if (mouseDown && GUILayoutUtility.GetLastRect().Contains(mousePos))
                        {
                            GUI.FocusControl(null);
                        }
                    }

                    if (GUILayout.Button("Text Replacement"))
                    {
                        currentPanel = Panel.REPLACEMENT;
                        scrollPos = new Vector2();
                        if (mouseDown && GUILayoutUtility.GetLastRect().Contains(mousePos))
                        {
                            GUI.FocusControl(null);
                        }
                    }

                    if (GUILayout.Button("Tools"))
                    {
                        currentPanel = Panel.TOOLS;
                        scrollPos = new Vector2();
                        if (mouseDown && GUILayoutUtility.GetLastRect().Contains(mousePos))
                        {
                            GUI.FocusControl(null);
                        }
                    }
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                {
                    GUILayout.Space(20);

                    scrollPos = GUILayout.BeginScrollView(scrollPos);
                    {
                        DisplayEmulatorPanel();
                        DisplayToolsPanel();
                        DisplayAccountsPanel();
                        DisplayTextReplacementPanel();
                    }
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }

        private void CheckForFlowExecutable()
        {
            flowExecutableFound = false;

            string flowExecutablePath = FlowControl.FindFlowExecutable();
            if (!string.IsNullOrEmpty(flowExecutablePath))
            {
                ProcessStartInfo psi = new ProcessStartInfo(flowExecutablePath)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                lastFlowCheck = DateTime.Now;

                try
                {
                    Process emulatorProcess = Process.Start(psi);
                    emulatorProcess.WaitForExit();
                }
                catch
                {
                    flowExecutableFound = false;
                    return;
                }

                flowExecutableFound = true;
            }
        }

        private void DisplayToolsPanel()
        {
            GUIStyle hidden = new GUIStyle();
            hidden.fixedHeight = 1f;
            hidden.clipping = TextClipping.Clip;

            if (currentPanel != Panel.TOOLS)
            {
                EditorGUILayout.BeginScrollView(Vector2.zero, hidden);
            }

            #region Header

            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            style.margin.left = 20;

            GUILayout.Label("Tools", style);
            HorizontalLine(Color.white);
            #endregion

            style.fontSize = 14;

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space(20);

                EditorGUILayout.BeginVertical();
                {

                    #region Contracts
                    EditorGUILayout.LabelField("Manage Contracts", style);

                    style.fontSize = 10;

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Contract Name: ", style, GUILayout.Width(100));
                        toolsContractName = EditorGUILayout.TextField("", toolsContractName);
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Contract: ", style, GUILayout.Width(100));
                        toolsContractScript = EditorGUILayout.ObjectField("", toolsContractScript, typeof(CadenceContractAsset), false, GUILayout.Width(300)) as CadenceContractAsset;
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Account: ", style, GUILayout.Width(100));
                        toolsContractAccountIndex = EditorGUILayout.Popup("", toolsContractAccountIndex, FlowControl.Data.Accounts.Select(x => x.Name).ToArray(), GUILayout.Width(300));
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUI.enabled = (toolsContractName != "" && toolsContractScript != null);
                        if (GUILayout.Button("Deploy Contract", GUILayout.Width(150)))
                        {
                            toolsContractResult = null;
                            toolsContractErrorLogged = false;
                            FlowSDK.GetWalletProvider().Authenticate(FlowControl.Data.Accounts.ToArray()[toolsContractAccountIndex].Name, null, null);
                            toolsContractResponse = FlowControl.Data.Accounts.ToArray()[toolsContractAccountIndex].DeployContract(toolsContractName, toolsContractScript.text);
                        }

                        if (GUILayout.Button("Update Contract", GUILayout.Width(150)))
                        {
                            toolsContractResult = null;
                            toolsContractErrorLogged = false;
                            FlowSDK.GetWalletProvider().Authenticate(FlowControl.Data.Accounts.ToArray()[toolsContractAccountIndex].Name, null, null);
                            toolsContractResponse = FlowControl.Data.Accounts.ToArray()[toolsContractAccountIndex].UpdateContract(toolsContractName, toolsContractScript.text);
                        }

                        if (GUILayout.Button("Remove Contract", GUILayout.Width(150)))
                        {
                            toolsContractResult = null;
                            toolsContractErrorLogged = false;
                            FlowSDK.GetWalletProvider().Authenticate(FlowControl.Data.Accounts.ToArray()[toolsContractAccountIndex].Name, null, null);
                            toolsContractResponse = FlowControl.Data.Accounts.ToArray()[toolsContractAccountIndex].RemoveContract(toolsContractName);
                        }

                        GUI.enabled = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (toolsContractResponse != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        string response = "Executing Transaction...";
                        if (toolsContractResponse.IsCompleted)
                        {
                            if (toolsContractResponse.Result.Error != null)
                            {
                                response = $"Error:  See console log for details.";
                                if (!toolsContractErrorLogged)
                                {
                                    Debug.LogError(toolsContractResponse.Result.Error.Message);
                                    toolsContractErrorLogged = true;
                                }
                            }
                            else
                            {
                                if (toolsContractResult == null)
                                {
                                    toolsContractResult = Transactions.GetResult(toolsContractResponse.Result.Id);
                                }

                                if (toolsContractResult.IsCompleted)
                                {
                                    if (toolsContractResult.Result.Error != null)
                                    {
                                        response = $"Error:  See console log for details.";
                                        if (!toolsContractErrorLogged)
                                        {
                                            Debug.LogError(toolsContractResult.Result.Error.Message);
                                            toolsContractErrorLogged = true;
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(toolsContractResult.Result.ErrorMessage))
                                    {
                                        response = $"Error:  See console log for details.";
                                        if (!toolsContractErrorLogged)
                                        {
                                            Debug.LogError(toolsContractResult.Result.ErrorMessage);
                                            toolsContractErrorLogged = true;
                                        }
                                    }
                                    else
                                    {
                                        if (toolsContractResult.Result.Status >= FlowTransactionStatus.SEALED)
                                        {
                                            response = "Completed";

                                            if (FlowControl.Data.Accounts.ToArray()[toolsContractAccountIndex].GatewayObject.Name == "Flow Testnet")
                                            {
                                                GUIStyle linkStyle = new GUIStyle();
                                                linkStyle.normal.textColor = new Color(6, 69, 173);
                                                linkStyle.active.textColor = linkStyle.normal.textColor;
                                                linkStyle.normal.background = null;
                                                linkStyle.margin.top = 10;
                                                
                                                string url = "https://testnet.flowscan.org/transaction/" + toolsContractResponse.Result.Id.ToString();

                                                if (GUILayout.Button("Status: " + url, linkStyle))
                                                {
                                                    Application.OpenURL(url);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            // reset result so that we can poll for it again
                                            toolsContractResult = null;
                                        }
                                    }
                                }
                            }

                        }
                        
                        EditorGUILayout.LabelField(response+"\n", style);
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion Contracts

                    EditorGUILayout.Space(20);

                    #region Transactions
                    style.fontSize = 14;
                    
                    EditorGUILayout.LabelField("Transactions", style);

                    style.fontSize = 10;

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Transaction: ", style, GUILayout.Width(100));
                        toolsTransactionScript = EditorGUILayout.ObjectField("", toolsTransactionScript, typeof(CadenceTransactionAsset), false, GUILayout.Width(300)) as CadenceTransactionAsset;
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Signer: ", style, GUILayout.Width(100));
                        toolsTransactionAccountIndex = EditorGUILayout.Popup("", toolsTransactionAccountIndex, FlowControl.Data.Accounts.Select(x => x.Name).ToArray(), GUILayout.Width(300));
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUI.enabled = (toolsTransactionScript != null);
                        if (GUILayout.Button("Execute Transaction", GUILayout.Width(150)))
                        {
                            string name = FlowControl.Data.Accounts.ToArray()[toolsTransactionAccountIndex].Name;
                            FlowSDK.GetWalletProvider().Authenticate(name, null, null);
                            toolsTransactionResult = null;
                            toolsTransactionErrorLogged = false;
                            toolsTransactionResponse = FlowControl.Data.Accounts.ToArray()[toolsTransactionAccountIndex].Submit(toolsTransactionScript.text);
                        }
                        GUI.enabled = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (toolsTransactionResponse != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        string response = "Executing Transaction...";
                        if (toolsTransactionResponse.IsCompleted)
                        {
                            if (toolsTransactionResponse.Result.Error != null)
                            {
                                response = $"Error:  See console log for details.";
                                if (!toolsTransactionErrorLogged)
                                {
                                    Debug.LogError(toolsTransactionResponse.Result.Error.Message);
                                    toolsTransactionErrorLogged = true;
                                }
                            }
                            else
                            {
                                if (toolsTransactionResult == null)
                                {
                                    toolsTransactionResult = Transactions.GetResult(toolsTransactionResponse.Result.Id);
                                }

                                if (toolsTransactionResult.IsCompleted)
                                {
                                    if (toolsTransactionResult.Result.Error != null)
                                    {
                                        response = $"Error:  See console log for details.";
                                        if (!toolsTransactionErrorLogged)
                                        {
                                            Debug.LogError(toolsTransactionResult.Result.Error.Message);
                                            toolsTransactionErrorLogged = true;
                                        }
                                    }
                                    else if (!string.IsNullOrEmpty(toolsTransactionResult.Result.ErrorMessage))
                                    {
                                        response = $"Error:  See console log for details.";
                                        if (!toolsTransactionErrorLogged)
                                        {
                                            Debug.LogError(toolsTransactionResult.Result.ErrorMessage);
                                            toolsTransactionErrorLogged = true;
                                        }
                                    }
                                    else
                                    {
                                        if (toolsTransactionResult.Result.Status >= FlowTransactionStatus.SEALED)
                                        {
                                            response = "Completed";

                                            if (FlowControl.Data.Accounts.ToArray()[toolsTransactionAccountIndex].GatewayObject.Name == "Flow Testnet")
                                            {
                                                GUIStyle linkStyle = new GUIStyle();
                                                linkStyle.normal.textColor = new Color(6, 69, 173);
                                                linkStyle.active.textColor = linkStyle.normal.textColor;
                                                linkStyle.normal.background = null;
                                                linkStyle.margin.top = 10;

                                                string url = "https://testnet.flowscan.org/transaction/" + toolsTransactionResponse.Result.Id.ToString();

                                                if (GUILayout.Button("Status: " + url, linkStyle))
                                                {
                                                    Application.OpenURL(url);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            
                            
                        }
                        EditorGUILayout.LabelField(response, style);
                        EditorGUILayout.EndHorizontal();
                    }
                    #endregion Transactions

                    EditorGUILayout.Space(20);

                    #region CreateAccount
                    style.fontSize = 14;
                    EditorGUILayout.LabelField("Create New Account", style);
                    style.fontSize = 10;

                    string[] availableAccounts = FlowControl.Data.Accounts.Select(x => x.Name).ToArray();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Paying Account: ", style, GUILayout.Width(100));
                        toolsCreateAccountPayerIndex = EditorGUILayout.Popup("", toolsCreateAccountPayerIndex, availableAccounts, GUILayout.Width(300));
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("New account name: ", style, GUILayout.Width(100));
                        toolsCreateAccountNewAccountName = EditorGUILayout.TextField("", toolsCreateAccountNewAccountName);
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                    
                    if (newAccountTask != null)
                    {
                        creationStatus =  "Creating account...";
                        GUI.enabled = false;
                        if (newAccountTask.IsCompleted)
                        {
                            if (newAccountTask.Result.Error == null)
                            {
                                SdkAccount newAccount = newAccountTask.Result;

                                FlowControl.Account newFCDAccount = new FlowControl.Account();
                                newFCDAccount.Name = newAccount.Name;
                                newFCDAccount.GatewayName = FlowControl.Data.Accounts[toolsCreateAccountPayerIndex].GatewayName;
                                newFCDAccount.AccountConfig["Private Key"] = newAccount.PrivateKey;
                                newFCDAccount.AccountConfig["Address"] = newAccount.Address;
                                FlowControl.Data.Accounts.Add(newFCDAccount);
                                EditorUtility.SetDirty(FlowControl.Data);
#if UNITY_2020_3_OR_NEWER
                                AssetDatabase.SaveAssetIfDirty(FlowControl.Data);
#else
                                AssetDatabase.SaveAssets();
#endif
                                toolsCreateAccountNewAccountName = "";
                                creationStatus = "Account created";
                            }
                            else
                            {
                                creationStatus = "Error creating account, see Console log for details.";
                                Debug.LogError($"Error creating account: {newAccountTask.Result.Error.Message}");
                            }
                            
                            newAccountTask = null;
                        }
                    }

                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Create", GUILayout.Width(100)))
                        {
                            FlowControl.Data.Accounts[toolsCreateAccountPayerIndex].GatewayObject.Init(FlowControl.Data.Accounts[toolsCreateAccountPayerIndex].AccountConfig);
                            FlowSDK.GetWalletProvider().Authenticate(FlowControl.Data.Accounts[toolsCreateAccountPayerIndex].Name, null, null);
                            newAccountTask = CommonTransactions.CreateAccount(toolsCreateAccountNewAccountName);
                        }
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();

                    GUI.enabled = true;
                    EditorGUILayout.LabelField(creationStatus, style);
#endregion

                    EditorGUILayout.Space(20);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            if (currentPanel != Panel.TOOLS)
            {
                EditorGUILayout.EndScrollView();
            }
        }

        private void DisplayTextReplacementPanel()
        {
            GUIStyle hidden = new GUIStyle();
            hidden.fixedHeight = 1f;
            hidden.clipping = TextClipping.Clip;

            if (currentPanel != Panel.REPLACEMENT)
            {
                EditorGUILayout.BeginScrollView(Vector2.zero, hidden);
            }


            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            style.margin.left = 20;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Text Replacements", style);
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    Undo.RecordObject(FlowControl.Data, "Add Text Replacement");
                    FlowControl.Data.TextReplacements.Add(new FlowControl.TextReplacement());
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
            HorizontalLine(Color.white);

            style.fontSize = 10;

            EditorGUILayout.Space(30);
            #region TextReplacmeentList
            foreach (FlowControl.TextReplacement tr in FlowControl.Data.TextReplacements)
            {
                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Description:", style, GUILayout.Width(100));
                    string newDescription = EditorGUILayout.TextField(tr.description);
                    if (newDescription != tr.description)
                    {
                        Undo.RecordObject(FlowControl.Data, "change description");
                        tr.description = newDescription;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Original Text:", style, GUILayout.Width(100));
                    string newOriginalText = EditorGUILayout.TextField(tr.originalText);
                    if (newOriginalText != tr.originalText)
                    {
                        Undo.RecordObject(FlowControl.Data, "change original text");
                        tr.originalText = newOriginalText;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Replacement Text:", style, GUILayout.Width(100));
                    string newReplacementText = EditorGUILayout.TextField(tr.replacementText);
                    if (newReplacementText != tr.replacementText)
                    {
                        Undo.RecordObject(FlowControl.Data, "change replacement text");
                        tr.replacementText = newReplacementText;
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Active:", style, GUILayout.Width(100));
                    bool newRtActive = EditorGUILayout.Toggle(tr.active);
                    if (newRtActive != tr.active)
                    {
                        Undo.RecordObject(FlowControl.Data, "change active");
                        tr.active = newRtActive;
                    }
                }
                GUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Apply to accounts:", style, GUILayout.Width(100));
                    if (GUILayout.Button(tr.ApplyToAccounts.Count == 0 ? "None" : string.Join(",", tr.ApplyToAccounts)))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("All"), tr.ApplyToAccounts.Contains("All"), OnTextReplacementAccountSelected, new TextReplacementAccountData(tr, "All"));
                        menu.AddItem(new GUIContent("None"), tr.ApplyToAccounts.Count == 0, OnTextReplacementAccountSelected, new TextReplacementAccountData(tr, "None"));
                        foreach (FlowControl.Account acct in FlowControl.Data.Accounts)
                        {
                            menu.AddItem(new GUIContent(acct.Name), tr.ApplyToAccounts.Contains(acct.Name), OnTextReplacementAccountSelected, new TextReplacementAccountData(tr, acct.Name));
                        }
                        menu.ShowAsContext();
                    }
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Apply to gateways:", style, GUILayout.Width(100));
                    if (GUILayout.Button(tr.ApplyToGateways.Count == 0 ? "None" : string.Join(",", tr.ApplyToGateways)))
                    {
                        GenericMenu menu = new GenericMenu();
                        menu.AddItem(new GUIContent("All"), tr.ApplyToGateways.Contains("All"), OnTextReplacementGatewaySelected, new TextReplacementGatewayData(tr, "All"));
                        menu.AddItem(new GUIContent("None"), tr.ApplyToGateways.Count == 0, OnTextReplacementGatewaySelected, new TextReplacementGatewayData(tr, "None"));
                        foreach (KeyValuePair<string, Gateway> gw in FlowControl.GatewayCache)
                        {
                            menu.AddItem(new GUIContent(gw.Key), tr.ApplyToGateways.Contains(gw.Key), OnTextReplacementGatewaySelected, new TextReplacementGatewayData(tr, gw.Key));
                        }
                        menu.ShowAsContext();
                    }
                }
                EditorGUILayout.EndHorizontal();


                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                    {
                        Undo.RecordObject(FlowControl.Data, "Delete Replacement Text");
                        FlowControl.Data.TextReplacements.Remove(tr);
                        GUILayout.EndHorizontal();
                        break;
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space(10);
            }
            #endregion
            
            if (currentPanel != Panel.REPLACEMENT)
            {
                EditorGUILayout.EndScrollView();
            }
        }

        private void DisplayAccountsPanel()
        {
            GUIStyle hidden = new GUIStyle();
            hidden.fixedHeight = 1f;
            hidden.clipping = TextClipping.Clip;

            if (currentPanel != Panel.ACCOUNTS)
            {
                EditorGUILayout.BeginScrollView(Vector2.zero, hidden);
            }

            #region Header

            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.fontStyle = FontStyle.Bold;
            style.normal.textColor = Color.white;
            style.margin.left = 20;

            GUIStyle linkStyle = new GUIStyle();
            linkStyle.normal.textColor = new Color(6, 69, 173);
            linkStyle.active.textColor = linkStyle.normal.textColor;
            linkStyle.normal.background = null;
            linkStyle.margin.top = 10;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Accounts", style);
                if (GUILayout.Button("+", GUILayout.Width(20)))
                {
                    Undo.RecordObject(FlowControl.Data, "Add account");
                    FlowControl.Data.Accounts.Add(new FlowControl.Account());
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            style.fontSize = 10;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space(40);
                GUILayout.Label("When starting the emulator, an emulator_service_account account will automatically be created/updated for you.", new GUIStyle() { wordWrap = true, normal = new GUIStyleState() { textColor = Color.white } });
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.Space(40);
                GUILayout.Label("Only accounts used for development on emulator/testnet should be stored here.  Do not store production mainnet keys here.", new GUIStyle() { wordWrap = true, fontStyle = FontStyle.Bold, normal = new GUIStyleState() { textColor = Color.red } });
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();

            HorizontalLine(Color.white);
#endregion

            EditorGUILayout.Space(20);

#region AccountList
            foreach (FlowControl.Account account in FlowControl.Data.Accounts)
            {
                if (FlowControl.GatewayCache.Count > 0)
                {
                    if (!FlowControl.GatewayCache.ContainsKey(account.GatewayName))
                    {
                        account.GatewayName = FlowControl.GatewayCache.Keys.ToArray()[0];
                    }
                }

                Gateway tempGateway = FlowControl.GatewayCache[account.GatewayName];

                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Gateway", style, GUILayout.Width(150));
                    account.GatewayName = FlowControl.GatewayCache.Keys.ToList<string>()[EditorGUILayout.Popup(FlowControl.GatewayCache.Keys.ToList<string>().IndexOf(account.GatewayName), FlowControl.GatewayCache.Keys.ToArray(), GUILayout.Width(200))];
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("Name", style, GUILayout.Width(150));

                    string newAccountName = EditorGUILayout.TextField(account.Name);
                    if (newAccountName != account.Name)
                    {
                        Undo.RecordObject(FlowControl.Data, "account name");
                        account.Name = newAccountName;
                    }
                }
                GUILayout.EndHorizontal();

                foreach (string field in tempGateway.RequiredParameters)
                {
                    if (!account.AccountConfig.ContainsKey(field))
                    {
                        account.AccountConfig[field] = "";
                    }

                    GUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.LabelField(field, style, GUILayout.Width(150));

                        if (tempGateway.SelectionParameters != null &&
                            tempGateway.SelectionParameters.ContainsKey(field) &&
                            tempGateway.SelectionParameters.Count > 0)
                        {
                            int selectedIndex = tempGateway.SelectionParameters[field].IndexOf(account.AccountConfig[field]);

                            if (selectedIndex == -1)
                            {
                                selectedIndex = 0;
                            }

                            string[] options = tempGateway.SelectionParameters[field].ToArray<string>();

                            selectedIndex = EditorGUILayout.Popup(selectedIndex, options);
                            Undo.RecordObject(FlowControl.Data, $"Change account {field}");
                            account.AccountConfig[field] = options[selectedIndex];
                        }
                        else
                        {
                            string newAccountConfigField = EditorGUILayout.TextField(account.AccountConfig[field]);
                            if (account.AccountConfig[field] != newAccountConfigField)
                            {
                                Undo.RecordObject(FlowControl.Data, $"Change account {field}");
                                account.AccountConfig[field] = newAccountConfigField;
                            }
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                if (tempGateway.Name == "Flow Testnet")
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.Space(10);

                        if (GUILayout.Button("https://testnet.flowscan.org/account/" + account.AccountConfig["Address"], linkStyle))
                        {
                            Application.OpenURL("https://testnet.flowscan.org/account/" + account.AccountConfig["Address"]);
                        }

                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Remove", GUILayout.Width(60)))
                    {
                        int accountIndex = FlowControl.Data.Accounts.FindIndex(item => item == account);

                        FlowControl.Data.Accounts.Remove(account);
                        GUILayout.EndHorizontal();

                        // correct interface indexes on account removal
                        Func<int, int, int> CorrectIndexes = (input, accountIndex) =>
                        {
                            int output = input;
                            if (output == accountIndex) output = 0;
                            if (output > FlowControl.Data.Accounts.Count) output = 0;
                            if (output > accountIndex) output--;
                            return output;
                        };
                        toolsContractAccountIndex = CorrectIndexes(toolsContractAccountIndex, accountIndex);
                        toolsTransactionAccountIndex = CorrectIndexes(toolsTransactionAccountIndex, accountIndex);
                        toolsCreateAccountPayerIndex = CorrectIndexes(toolsCreateAccountPayerIndex, accountIndex);

                        break;
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUILayout.Space(30);
            }
#endregion

            if (currentPanel != Panel.ACCOUNTS)
            {
                EditorGUILayout.EndScrollView();
            }
        }

        private void DisplayEmulatorPanel()
        {
            GUIStyle hidden = new GUIStyle
            {
                fixedHeight = 1f,
                clipping = TextClipping.Clip
            };

            if (currentPanel != Panel.EMULATOR)
            {
                EditorGUILayout.BeginScrollView(Vector2.zero, hidden);
            }

            if (!flowExecutableFound)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Flow executable not found, click here for installation instructions:");
                if (GUILayout.Button("Install", GUILayout.Width(100)))
                {
                    Application.OpenURL("https://developers.flow.com/tools/flow-cli/install");
                }

                EditorGUILayout.EndHorizontal();
            }
            
            #region EmulatorDataDirectoryRow
            GUILayout.BeginHorizontal();
            {

                GUIStyle emulatorDataDirectoryLabelStyle = EditorStyles.label;
                emulatorDataDirectoryLabelStyle.richText = true;

                bool dataDirectoryValid = FlowControl.Data.EmulatorSettings.emulatorDataDirectory != "" && Directory.Exists(FlowControl.Data.EmulatorSettings.emulatorDataDirectory);

                if (dataDirectoryValid)
                {
                    GUILayout.Label($"Emulator data directory:", GUILayout.Width(200));
                }
                else
                {
                    GUILayout.Label($"<color=red>Emulator data directory (required):</color>", emulatorDataDirectoryLabelStyle, GUILayout.Width(200));
                }


                string newEmulatorDataDir = EditorGUILayout.TextField(FlowControl.Data.EmulatorSettings.emulatorDataDirectory);

                if (GUILayout.Button("...", GUILayout.Width(20)))
                {
                    string selectedEmulatorDataDirectory = EditorUtility.OpenFolderPanel("Emulator data directory", FlowControl.Data.EmulatorSettings.emulatorDataDirectory, "");
                    if (selectedEmulatorDataDirectory != "")
                    {
                        newEmulatorDataDir = selectedEmulatorDataDirectory;
                    }
                }

                if (newEmulatorDataDir != FlowControl.Data.EmulatorSettings.emulatorDataDirectory)
                {
                    Undo.RecordObject(FlowControl.Data, "change data path");
                    FlowControl.Data.EmulatorSettings.emulatorDataDirectory = newEmulatorDataDir;

#if UNITY_2020_3_OR_NEWER
                    AssetDatabase.SaveAssetIfDirty(FlowControl.Data);
#else
                    AssetDatabase.SaveAssets();
#endif
                }

                GUILayout.Space(20);
            }
            
            GUILayout.EndHorizontal();
            #endregion

            #region Emulator endpoint
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label($"Emulator endpoint:", GUILayout.Width(200));
                EditorGUI.BeginChangeCheck();
                string newEmulatorEndpoint = GUILayout.TextField(FlowControl.Data.EmulatorSettings.emulatorEndpoint);
                if (newEmulatorEndpoint != FlowControl.Data.EmulatorSettings.emulatorEndpoint)
                {
                    Undo.RecordObject(FlowControl.Data, "Changed emulator endpoint");
                    FlowControl.Data.EmulatorSettings.emulatorEndpoint = newEmulatorEndpoint;
                    #if UNITY_2020_3_OR_NEWER
                    AssetDatabase.SaveAssetIfDirty(FlowControl.Data);
                    #else
                    AssetDatabase.SaveAssets();
                    #endif
                }
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            #region EmulatorOnStartRow
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Run emulator in play mode?", GUILayout.Width(200));
                bool newREIPM = GUILayout.Toggle(FlowControl.Data.EmulatorSettings.runEmulatorInPlayMode, "");
                if (newREIPM != FlowControl.Data.EmulatorSettings.runEmulatorInPlayMode)
                {
                    Undo.RecordObject(FlowControl.Data, "Change emulator setting");
                    FlowControl.Data.EmulatorSettings.runEmulatorInPlayMode = newREIPM;
                }
            }
            GUILayout.EndHorizontal();
#endregion

            #region EmulatorControl
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 18;
            style.fontStyle = FontStyle.Bold;

            GUILayout.Space(20);
            GUILayout.Label("Emulator Control", style);
            GUILayout.Space(20);
            style.fontSize = 12;
            style.fontStyle = FontStyle.Normal;
            style.wordWrap = true;
            GUILayout.Label("You can start and stop the emulator in edit mode if you want to populate it with contracts or accounts.", style);

            GUILayout.Space(20);


#region StartEmulatorButton
            if (FlowControl.IsEmulatorRunning || string.IsNullOrEmpty(FlowControl.Data.EmulatorSettings.emulatorDataDirectory) || 
                !Directory.Exists(FlowControl.Data.EmulatorSettings.emulatorDataDirectory) || !flowExecutableFound)
            {
                GUI.enabled = false;
            }
            
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Start Emulator", GUILayout.Width(200)))
                {
                    FlowControl.StartEmulator();
                }

                GUI.enabled = true;

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
#endregion

#region EmulatorStopButton
            if (!FlowControl.IsEmulatorRunning)
            {
                GUI.enabled = false;
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Stop Emulator", GUILayout.Width(200)))
                {
                    FlowControl.StopEmulator();
                }

                GUI.enabled = true;

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
#endregion

#region EmulatorClearDataButton
            if (FlowControl.IsEmulatorRunning)
            {
                GUI.enabled = false;
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear Persistent Data", GUILayout.Width(200)))
                {
                    FlowControl.ClearEmulatorData();
                }

                GUI.enabled = true;

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
#endregion

#region ShowEmulatorLogButton
            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Show Emulator Log", GUILayout.Width(200)))
                {
                    FlowOutputWindow.ShowWindow();
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();
#endregion


            GUILayout.Label("Emulator Status:  " + (FlowControl.IsEmulatorRunning ? "Running" : "Stopped"));


            if (currentPanel != Panel.EMULATOR)
            {
                EditorGUILayout.EndScrollView();
            }
            #endregion
        }

        private void HorizontalLine(Color color)
        {
            GUIStyle horizontalLine = new GUIStyle
            {
                normal =
                {
                    background = EditorGUIUtility.whiteTexture
                },
                margin = new RectOffset(5, 5, 4, 4),
                fixedHeight = 1
            };
            Color c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, horizontalLine);
            GUI.color = c;
        }

        private void OnLostFocus()
        {
            EditorUtility.SetDirty(FlowControl.Data);
            AssetDatabase.SaveAssets();
        }

        private class TextReplacementAccountData
        {
            public FlowControl.TextReplacement textReplacement;
            public string account;

            public TextReplacementAccountData(FlowControl.TextReplacement textReplacement, string account)
            {
                this.textReplacement = textReplacement;
                this.account = account;
            }
        }

        private class TextReplacementGatewayData
        {
            public FlowControl.TextReplacement textReplacement;
            public string gateway;

            public TextReplacementGatewayData(FlowControl.TextReplacement textReplacement, string gateway)
            {
                this.textReplacement = textReplacement;
                this.gateway = gateway;
            }
        }

        private void OnTextReplacementAccountSelected(object trData)
        {
            Undo.RecordObject(FlowControl.Data, "change account filter");
            TextReplacementAccountData data = trData as TextReplacementAccountData;

            if (data.account == "All")
            {
                data.textReplacement.ApplyToAccounts.Clear();
                data.textReplacement.ApplyToAccounts.Add("All");
                return;
            } 
            else
            {
                data.textReplacement.ApplyToAccounts.Remove("All");
            }

            if (data.account == "None")
            {
                data.textReplacement.ApplyToAccounts.Clear();
                return;
            }


            if (data.textReplacement.ApplyToAccounts.Contains(data.account))
            {
                data.textReplacement.ApplyToAccounts.Remove(data.account);
            }
            else
            {
                data.textReplacement.ApplyToAccounts.Add(data.account);
            }

            if (data.textReplacement.ApplyToAccounts.Count > 1)
            {
                data.textReplacement.ApplyToAccounts.Remove("All");
            }
        }

        private void OnTextReplacementGatewaySelected(object trData)
        {
            Undo.RecordObject(FlowControl.Data, "change gateway filter");
            TextReplacementGatewayData data = trData as TextReplacementGatewayData;


            if (data.gateway == "All")
            {
                data.textReplacement.ApplyToGateways.Clear();
                data.textReplacement.ApplyToGateways.Add("All");
                return;
            }
            else
            {
                data.textReplacement.ApplyToGateways.Remove("All");
            }

            if (data.gateway == "None")
            {
                data.textReplacement.ApplyToGateways.Clear();
                return;
            }


            if (data.textReplacement.ApplyToGateways.Contains(data.gateway))
            {
                data.textReplacement.ApplyToGateways.Remove(data.gateway);
            }
            else
            {
                data.textReplacement.ApplyToGateways.Add(data.gateway);
            }

            if (data.textReplacement.ApplyToGateways.Count > 1)
            {
                data.textReplacement.ApplyToGateways.Remove("All");
            }
        }

        private void OnInspectorUpdate()
        {
            if (!flowExecutableFound && (DateTime.Now - lastFlowCheck) > TimeSpan.FromSeconds(5))
            {
                CheckForFlowExecutable();
            }
        }
    }
}