#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(GoogleSheetManager))]
public class GenerateButton : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GoogleSheetManager googleSheetManager = (GoogleSheetManager)target;

        if (GUILayout.Button("1.Language"))
        {
            googleSheetManager.OnClickPartSave("Language");
        }
        else if (GUILayout.Button("2.Story"))
        {
            googleSheetManager.OnClickPartSave("Story");
        }
        else if (GUILayout.Button("3.Select"))
        {
            googleSheetManager.OnClickPartSave("Select");
        }
        else if (GUILayout.Button("4.NameColor"))
        {
            googleSheetManager.OnClickPartSave("NameColor");
        }
        // else if (GUILayout.Button("DialogueText"))
        // {
        //     googleSheetManager.OnClickPartSave("DialogueText");
        // }
    }
}
#endif

