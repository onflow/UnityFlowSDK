using UnityEditor;
using UnityEngine;

namespace DapperLabs.Flow.Sdk.Unity
{
    /// <summary>
    /// Shows emulator output.  Open with Window->Flow->Emulator Output
    /// </summary>
    public class FlowOutputWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private Font font;
        private bool wordWrap;

        /// <summary>
        /// Shows the Emulator Output window
        /// </summary>
        [MenuItem("Window/Flow/Emulator Output")]
        public static void ShowWindow()
        {
            GetWindow<FlowOutputWindow>("Flow Emulator Output");
        }

        private void CreateGUI()
        {
            font = Resources.Load<Font>("FiraCode-VariableFont_wght");
        }

        private void OnGUI()
        {
            GUIStyle flowLabelStyle = new GUIStyle(GUI.skin.GetStyle("label"))
            {
                wordWrap = wordWrap,
                richText = true,
                font = font
            };

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            if (FlowControl.EmulatorOutput != null)
            {
                GUILayout.TextArea(FlowControl.EmulatorOutput.Replace("\\r\\n", "\n").Replace("\\n", "\n"), flowLabelStyle);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button($"Wordwrap: {(wordWrap?"ON":"OFF")}", GUILayout.Width(100)))
                {
                    wordWrap = !wordWrap;
                }

                if (GUILayout.Button("Clear", GUILayout.Width(100)))
                {
                    FlowControl.ClearEmulatorOutput();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
