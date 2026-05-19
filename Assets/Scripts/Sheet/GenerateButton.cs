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
        // else if (GUILayout.Button("DialogueSet"))
        // {
        //     googleSheetManager.OnClickPartSave("DialogueSet");
        // }
        // else if (GUILayout.Button("DialogueChoiceSet"))
        // {
        //     googleSheetManager.OnClickPartSave("DialogueChoiceSet");
        // }
        // else if (GUILayout.Button("DialogueText"))
        // {
        //     googleSheetManager.OnClickPartSave("DialogueText");
        // }
    }
}
#endif

