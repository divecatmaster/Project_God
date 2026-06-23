using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GlitchEffectController))]
public class GlitchEffectControllerEditor : Editor
{
    private bool liveOverrideActive = false;
    private float liveIntensity = 0.5f;

    public override void OnInspectorGUI()
    {
        // Draw the standard inspector properties
        serializedObject.Update();
        
        GlitchEffectController controller = (GlitchEffectController)target;

        // Custom Title Header
        GUILayout.Space(10);
        EditorGUILayout.BeginVertical("box");
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 14;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = new Color(0.0f, 0.9f, 0.9f); // Cyan cyan
        EditorGUILayout.LabelField("⚡ CYBER GLITCH CONTROLLER ⚡", headerStyle);
        EditorGUILayout.EndVertical();
        GUILayout.Space(5);

        // Draw Default Fields
        DrawDefaultInspector();

        GUILayout.Space(15);

        // --- RUNTIME TESTING TOOLS (ONLY VISIBLE IN PLAY MODE) ---
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical("box");
            
            GUIStyle sectionHeader = new GUIStyle(EditorStyles.boldLabel);
            sectionHeader.normal.textColor = new Color(1.0f, 0.2f, 0.6f); // Pink
            EditorGUILayout.LabelField("🎮 Runtime Interactive Debugger", sectionHeader);
            GUILayout.Space(5);

            // 1. Live Slider Override
            EditorGUI.BeginChangeCheck();
            liveOverrideActive = EditorGUILayout.Toggle("Enable Live Slider Override", liveOverrideActive);
            if (liveOverrideActive)
            {
                EditorGUI.indentLevel++;
                liveIntensity = EditorGUILayout.Slider("Live Glitch Intensity", liveIntensity, 0f, 1f);
                EditorGUI.indentLevel--;
                
                // Set the intensity directly
                controller.currentIntensity = liveIntensity;
                
                // If Custom Sliders are active, the controller already updates the material in Update().
                // If another preset is selected, let's force it to update the shader weights based on the preset.
                if (controller.activeGlitchPreset != GlitchEffectController.GlitchType.CustomSliders)
                {
                    // Call TriggerGlitch with duration=0 to hold the state, or we let the slider control it.
                    // To make it simple: when live override is active, we force the current preset weights.
                    // We can access this via Reflection or just by setting weights if we edit the script,
                    // but since the Update loop applies custom sliders, the user can switch to CustomSliders preset to test weights.
                    if (controller.activeGlitchPreset == GlitchEffectController.GlitchType.CustomSliders)
                    {
                        EditorGUILayout.HelpBox("Tweak the 'Custom Preset Sliders' above to see each effect live!", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"Currently previewing Preset: {controller.activeGlitchPreset}. Switch to 'CustomSliders' to adjust individual sliders!", MessageType.Info);
                    }
                }
            }
            else
            {
                // If turned off, resume normal behaviour
                if (EditorGUI.EndChangeCheck())
                {
                    controller.currentIntensity = 0f;
                }
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Manual Glitch Triggers (Click to Fire Burst):", EditorStyles.miniBoldLabel);

            // Grid Layout for Buttons
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Full Chaos Burst", GUILayout.Height(30)))
            {
                controller.TriggerGlitch(GlitchEffectController.GlitchType.FullChaos, 0.8f, 0.9f);
                liveOverrideActive = false;
            }
            if (GUILayout.Button("Block Shift Burst", GUILayout.Height(30)))
            {
                controller.TriggerGlitch(GlitchEffectController.GlitchType.BlockShiftOnly, 0.6f, 0.8f);
                liveOverrideActive = false;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("RGB Split Burst", GUILayout.Height(30)))
            {
                controller.TriggerGlitch(GlitchEffectController.GlitchType.RGBSplitOnly, 0.5f, 0.8f);
                liveOverrideActive = false;
            }
            if (GUILayout.Button("Static Noise Burst", GUILayout.Height(30)))
            {
                controller.TriggerGlitch(GlitchEffectController.GlitchType.StaticNoiseOnly, 0.7f, 0.6f);
                liveOverrideActive = false;
            }
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Color Invert Flash Burst", GUILayout.Height(30)))
            {
                controller.TriggerGlitch(GlitchEffectController.GlitchType.ColorInvertOnly, 0.4f, 1.0f);
                liveOverrideActive = false;
            }

            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("Enter Play Mode to access real-time Glitch Burst Triggers and Live Sliders!", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}