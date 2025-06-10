#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterDataSO))]
public class MonsterDataSOEditor : Editor
{
    // Organize foldouts by category and set sensible defaults
    private bool showBasicInfo = true;
    private bool showStats = true;
    private bool showBehavior = false;          // Combine happiness + poop
    private bool showEvolution = false;         // Less frequently edited
    private bool showAnimations = false;        // Technical details - collapsed by default
    private bool showAudio = false;             // Less frequently edited
    private bool showVisuals = false;           // Less frequently edited

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // More compact header
        DrawBasicInfo();
        DrawStats();
        DrawBehaviorSettings();
        DrawEvolutionSettings();
        DrawAnimationSettings();
        DrawVisualSettings();
        DrawAudioSettings();

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawBasicInfo()
    {
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "Basic Info", true);
        if (!showBasicInfo) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monPrice"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monType"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawStats()
    {
        showStats = EditorGUILayout.Foldout(showStats, "Stats & Movement", true);
        if (!showStats) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpd"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("foodDetectionRange"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("eatDistance"));
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Base Values", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseHunger"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseHappiness"));
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawBehaviorSettings()
    {
        showBehavior = EditorGUILayout.Foldout(showBehavior, "Behavior Settings", true);
        if (!showBehavior) return;

        EditorGUI.indentLevel++;
        
        // Hunger & Happiness
        EditorGUILayout.LabelField("Hunger & Happiness", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hungerDepleteRate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pokeCooldownDuration"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("areaHappinessRate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pokeHappinessValue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hungerHappinessThreshold"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hungerHappinessDrainRate"));
        
        EditorGUILayout.Space(3);
        
        // Poop Behavior
        EditorGUILayout.LabelField("Poop Behavior", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("poopRate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("clickToCollectPoop"));
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawEvolutionSettings()
    {
        showEvolution = EditorGUILayout.Foldout(showEvolution, "Evolution System", true);
        if (!showEvolution) return;

        EditorGUI.indentLevel++;
        
        // Basic Evolution Settings
        EditorGUILayout.LabelField("Basic Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("canEvolve"));
        
        var canEvolve = serializedObject.FindProperty("canEvolve").boolValue;
        if (canEvolve)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isEvolved"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isFinalEvol"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("evolutionLevel"));
            
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Requirements & Behaviors", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("evolutionRequirements"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("evolutionBehaviors"), true);
        }
        else
        {
            EditorGUILayout.HelpBox("Evolution is disabled for this monster.", MessageType.Info);
        }
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawAnimationSettings()
    {
        showAnimations = EditorGUILayout.Foldout(showAnimations, "Animation & Spine Data", true);
        if (!showAnimations) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterSpine"), true);
        
        EditorGUILayout.Space(3);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("evolutionAnimationSets"), true);
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawVisualSettings()
    {
        showVisuals = EditorGUILayout.Foldout(showVisuals, "Visual Assets", true);
        if (!showVisuals) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsImgs"), true);
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawAudioSettings()
    {
        showAudio = EditorGUILayout.Foldout(showAudio, "Audio Settings", true);
        if (!showAudio) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("idleSounds"), true);
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Event Sounds", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("happySound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("eatSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hurtSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactionSound"));
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Special Event Sounds", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("evolveSound"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("deathSound"));
        
        EditorGUI.indentLevel--;
    }
}
#endif