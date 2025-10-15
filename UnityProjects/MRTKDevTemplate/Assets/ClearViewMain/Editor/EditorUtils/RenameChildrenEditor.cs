// RenameChildrenEditor.cs  (put this in an 'Editor' folder)
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RenameChildren))]
[CanEditMultipleObjects]
public class RenameChildrenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(6);
        if (GUILayout.Button("Rename Children Now"))
        {
            foreach (var t in targets)
                ((RenameChildren)t).RenameNow();
        }
    }
}
#endif
