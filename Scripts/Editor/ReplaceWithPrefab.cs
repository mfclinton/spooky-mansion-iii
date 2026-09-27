using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefab : EditorWindow
{
    private string objectNameToReplace = "basic door frame.";
    private string prefabName = "basic door frame";

    [MenuItem("Tools/Replace With Prefab")]
    static void CreateReplaceWithPrefab()
    {
        EditorWindow.GetWindow<ReplaceWithPrefab>("Replace With Prefab");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace GameObjects with Prefab", EditorStyles.boldLabel);

        objectNameToReplace = EditorGUILayout.TextField("Name of Objects to Replace", objectNameToReplace);
        prefabName = EditorGUILayout.TextField("Name of Prefab", prefabName);

        if(GUILayout.Button("Replace"))
        {
            Replace();
        }
    }

    private void Replace()
    {
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogError("Prefab not found");
            return;
        }

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach(GameObject go in allObjects)
        {
            if(go.name.StartsWith(objectNameToReplace))
            {
                GameObject newObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if(newObject != null)
                {
                    newObject.transform.SetParent(go.transform.parent, true);
                    newObject.transform.localPosition = go.transform.localPosition;
                    newObject.transform.localRotation = go.transform.localRotation;
                    newObject.transform.localScale = go.transform.localScale;
                    DestroyImmediate(go);
                }
                else
                {
                    Debug.LogError("Could not instantiate prefab");
                }
            }
        }
    }
}
