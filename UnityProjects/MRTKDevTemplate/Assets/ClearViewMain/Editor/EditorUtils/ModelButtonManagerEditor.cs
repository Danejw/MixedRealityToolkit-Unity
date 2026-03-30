using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace ClearView.Editor
{
    [CustomEditor(typeof(ModelButtonManager))]
    public class ModelButtonManagerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);

            if (GUILayout.Button("Create Button List", GUILayout.Height(32)))
            {
                GenerateButtons((ModelButtonManager)target);
            }
        }

        private static void GenerateButtons(ModelButtonManager manager)
        {
            if (manager.ModelManager == null ||
                manager.ListItemPrefab == null ||
                manager.ToggleListParent == null ||
                manager.ToggleCollection == null)
            {
                Debug.LogError("ModelButtonManager is missing required references.", manager);
                return;
            }

            ClearGeneratedButtonsOnly(manager.ToggleListParent);

            SerializedObject modelManagerSO = new SerializedObject(manager.ModelManager);
            SerializedProperty availableModelsProp = modelManagerSO.FindProperty("availableModels");

            if (availableModelsProp == null || !availableModelsProp.isArray)
            {
                Debug.LogError("Could not find availableModels on ModelManager.", manager);
                return;
            }

            for (int i = 0; i < availableModelsProp.arraySize; i++)
            {
                SerializedProperty element = availableModelsProp.GetArrayElementAtIndex(i);
                GameObject modelPrefab = element.objectReferenceValue as GameObject;

                if (modelPrefab == null)
                    continue;

                int toggleIndex = i + 1; // keep manual None button at index 0 untouched
                CreateListItem(manager, modelPrefab.name, toggleIndex);
            }

            EditorUtility.SetDirty(manager.ToggleListParent);
            AssetDatabase.SaveAssets();
        }

        private static void CreateListItem(ModelButtonManager manager, string modelName, int index)
        {
            GameObject item = (GameObject)PrefabUtility.InstantiatePrefab(manager.ListItemPrefab, manager.ToggleListParent);
            item.name = modelName;

            RectTransform rect = item.GetComponent<RectTransform>();
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 170f);

            TMP_Text label = FindLabel(item.transform);
            if (label != null)
            {
                label.text = modelName;
                EditorUtility.SetDirty(label);
            }

            Component pressableButton = item.GetComponent("PressableButton");
            if (pressableButton == null)
            {
                Debug.LogWarning($"No PressableButton found on '{modelName}'.", item);
                return;
            }

            WirePressableButtonEvents(pressableButton, manager.ModelManager, manager.ToggleCollection, modelName, index);

            EditorUtility.SetDirty(item);
        }

        private static TMP_Text FindLabel(Transform root)
        {
            Transform textTransform = root.Find("Frontplate/AnimatedContent/Text");
            if (textTransform != null)
                return textTransform.GetComponent<TMP_Text>();

            return root.GetComponentInChildren<TMP_Text>(true);
        }

        private static void ClearGeneratedButtonsOnly(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 1; i--)
            {
                Object.DestroyImmediate(parent.GetChild(i).gameObject);
            }
        }

        private static void WirePressableButtonEvents(
            Component pressableButton,
            ModelManager modelManager,
            Component toggleCollection,
            string modelName,
            int index)
        {
            SerializedObject buttonSO = new SerializedObject(pressableButton);
            SerializedProperty activatedProp = buttonSO.FindProperty("m_Activated");

            if (activatedProp == null)
            {
                Debug.LogError("Could not find m_Activated on PressableButton.", pressableButton);
                return;
            }

            ClearUnityEvent(activatedProp);

            AddPersistentCall(
                activatedProp,
                modelManager,
                "SwitchTo",
                PersistentListenerMode.String,
                stringArg: modelName
            );

            AddPersistentCall(
                activatedProp,
                toggleCollection,
                "SetSelection",
                PersistentListenerMode.Int,
                intArg: index
            );

            buttonSO.ApplyModifiedPropertiesWithoutUndo();
        }

        private static SerializedProperty FindToggledUnityEvent(SerializedObject so)
        {
            SerializedProperty iterator = so.GetIterator();
            bool enterChildren = true;

            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                string display = iterator.displayName.ToLowerInvariant();
                string path = iterator.propertyPath.ToLowerInvariant();

                if (!display.Contains("toggled") && !path.Contains("toggled"))
                    continue;

                SerializedProperty calls = iterator.FindPropertyRelative("m_PersistentCalls.m_Calls");
                if (calls != null && calls.isArray)
                    return iterator.Copy();
            }

            return null;
        }

        private static void ClearUnityEvent(SerializedProperty unityEventProperty)
        {
            SerializedProperty calls = unityEventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
            if (calls != null && calls.isArray)
                calls.ClearArray();
        }

        private static void AddPersistentCall(
            SerializedProperty unityEventProperty,
            Object target,
            string methodName,
            PersistentListenerMode mode,
            int intArg = 0,
            string stringArg = "")
        {
            SerializedProperty calls = unityEventProperty.FindPropertyRelative("m_PersistentCalls.m_Calls");
            if (calls == null || !calls.isArray)
                return;

            int newIndex = calls.arraySize;
            calls.InsertArrayElementAtIndex(newIndex);

            SerializedProperty call = calls.GetArrayElementAtIndex(newIndex);

            call.FindPropertyRelative("m_Target").objectReferenceValue = target;
            call.FindPropertyRelative("m_TargetAssemblyTypeName").stringValue = target.GetType().AssemblyQualifiedName;
            call.FindPropertyRelative("m_MethodName").stringValue = methodName;
            call.FindPropertyRelative("m_Mode").intValue = (int)mode;
            call.FindPropertyRelative("m_CallState").intValue = (int)UnityEventCallState.RuntimeOnly;

            SerializedProperty args = call.FindPropertyRelative("m_Arguments");
            args.FindPropertyRelative("m_ObjectArgument").objectReferenceValue = null;
            args.FindPropertyRelative("m_ObjectArgumentAssemblyTypeName").stringValue = string.Empty;
            args.FindPropertyRelative("m_IntArgument").intValue = intArg;
            args.FindPropertyRelative("m_FloatArgument").floatValue = 0f;
            args.FindPropertyRelative("m_StringArgument").stringValue = stringArg;
            args.FindPropertyRelative("m_BoolArgument").boolValue = false;
        }
    }
}

