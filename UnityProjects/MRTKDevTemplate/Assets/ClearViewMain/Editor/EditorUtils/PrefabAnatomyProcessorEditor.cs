// PrefabAnatomyProcessorEditor.cs  (put this in an 'Editor' folder)
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PrefabAnatomyProcessor))]
[CanEditMultipleObjects]
public class PrefabAnatomyProcessorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(8);
        GUI.enabled = targets != null && targets.Length > 0;
        if (GUILayout.Button("Process Target Now"))
        {
            foreach (var t in targets)
                ((PrefabAnatomyProcessor)t).ProcessNow();
        }
        GUI.enabled = true;
    }
}
#endif
