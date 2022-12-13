using System;
using System.Collections.Generic;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.Unity
{
    /// <summary>
    /// Container that holds the data needed for FlowControl.  This is serialized into a FlowControlData resource and stored with your project.
    /// </summary>
    [Serializable]
    public class FlowControlData : ScriptableObject
    {
        [SerializeField]
        private FlowControl.EmulatorSettings _emulatorSettings;

        [SerializeField]
        private List<FlowControl.TextReplacement> _textReplacements;

        [SerializeField]
        private List<FlowControl.Account> _accounts;
        
        /// <summary>
        /// Stores emulator settings
        /// </summary>
        public FlowControl.EmulatorSettings EmulatorSettings
        {
            get
            {
                if (_emulatorSettings == null)
                {
                    _emulatorSettings = new FlowControl.EmulatorSettings();
                }

                return _emulatorSettings;
            }

            set => _emulatorSettings = value;
        }   

        /// <summary>
        /// Stores text replacement settings
        /// </summary>
        public List<FlowControl.TextReplacement> TextReplacements
        {
            get
            {
                if (_textReplacements == null)
                {
                    _textReplacements = new List<FlowControl.TextReplacement>();
                }

                return _textReplacements;
            }

            set => _textReplacements = value;
        }

        /// <summary>
        /// Stores accounts
        /// </summary>
        public List<FlowControl.Account> Accounts
        {
            get
            {
                if (_accounts == null)
                {
                    _accounts = new List<FlowControl.Account>();
                }

                return _accounts;
            }

            set => _accounts = value;
        }
    }
}