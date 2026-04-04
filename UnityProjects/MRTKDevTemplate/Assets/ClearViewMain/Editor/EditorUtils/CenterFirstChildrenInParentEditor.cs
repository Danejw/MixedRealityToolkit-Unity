using UnityEditor;
using UnityEngine;

namespace ClearView.Editor
{
    [CustomEditor(typeof(CenterFirstChildrenInParent))]
    public class CenterFirstChildrenInParentEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CenterFirstChildrenInParent script = (CenterFirstChildrenInParent)target;

            GUILayout.Space(8);

            if (GUILayout.Button("Center First Children In Parent"))
            {
                Undo.RecordObject(script.transform, "Center First Children");

                for (int i = 0; i < script.transform.childCount; i++)
                {
                    Undo.RecordObject(script.transform.GetChild(i), "Center First Children");
                }

                script.CenterChildren();
            }
        }
    }
}