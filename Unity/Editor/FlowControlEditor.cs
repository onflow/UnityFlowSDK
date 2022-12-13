using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace DapperLabs.Flow.Sdk.Unity
{
    /// <summary>
    /// FlowControl editor.  See <a href="md__flow_control.html">the FlowControl page</a> 
    /// </summary>
    [CustomEditor(typeof(FlowControl))]
    public class FlowControlEditor : Editor
    {
        public string json = "";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (GUILayout.Button("Open Flow Control Window"))
            {
                FlowControlWindow.ShowFlowControl();
            }

            if(GUILayout.Button("Log JSON"))
            {
                json = FlowControl.ToJson();
            }

            EditorGUI.BeginChangeCheck();
            json = GUILayout.TextArea(json);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(this, "Changed JSON");
            }

            if(GUILayout.Button("Load JSON"))
            {
                FlowControl.ClearData();
                JsonConvert.PopulateObject(json, FlowControl.Data);
                EditorUtility.SetDirty(FlowControl.Data);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

    }
}