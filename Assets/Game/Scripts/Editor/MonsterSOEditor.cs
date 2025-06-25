#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterDataSO))]
public class MonsterDataSOEditor : Editor
{
    // Organize foldouts by category and set sensible defaults
    private bool showBasicInfo = false;        // Changed from true to false
    private bool showStats = false;            // Changed from true to false
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
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Classification", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monType"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("poopType")); // Move this here
        
        // Pricing Section
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Pricing & Gacha", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterPrice"), new GUIContent("Buy Price"));
        
        // Sell prices
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sellPriceStage1"), new GUIContent("Sell Price (Stage 1)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sellPriceStage2"), new GUIContent("Sell Price (Stage 2)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("sellPriceStage3"), new GUIContent("Sell Price (Stage 3)"));
        
        // NEW: Gacha data
        EditorGUILayout.Space(2);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gachaChancePercent"), new GUIContent("Gacha Chance (Decimal)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gachaChanceDisplay"), new GUIContent("Gacha Chance (Display)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("isGachaOnly"), new GUIContent("Gacha Only"));
        
        EditorGUILayout.Space(3);
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawStats()
    {
        showStats = EditorGUILayout.Foldout(showStats, "Stats & Movement", true);
        if (!showStats) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealth"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpd"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("foodDetectionRange"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("eatDistance"));
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Base Values", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("startingHunger"));      // <-- FIXED
        EditorGUILayout.PropertyField(serializedObject.FindProperty("startingHappiness"));   // <-- FIXED
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Drop Rates", EditorStyles.boldLabel);
        
        // Stage 1 rates (always shown)
        EditorGUILayout.LabelField("Stage 1:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("goldCoinDropRateStage1"), new GUIContent("Gold Coin Rate (minutes)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("silverCoinDropRateStage1"), new GUIContent("Silver Coin Rate (minutes)"));
        EditorGUI.indentLevel--;
        
        // Stage 2 rates
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Stage 2:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("goldCoinDropRateStage2"), new GUIContent("Gold Coin Rate (minutes)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("silverCoinDropRateStage2"), new GUIContent("Silver Coin Rate (minutes)"));
        
        // Show fallback info
        var goldStage2 = serializedObject.FindProperty("goldCoinDropRateStage2").floatValue;
        var silverStage2 = serializedObject.FindProperty("silverCoinDropRateStage2").floatValue;
        if (goldStage2 <= 0) EditorGUILayout.HelpBox("Will use Stage 1 gold rate as fallback", MessageType.Info);
        if (silverStage2 <= 0) EditorGUILayout.HelpBox("Will use Stage 1 silver rate as fallback", MessageType.Info);
        EditorGUI.indentLevel--;
        
        // Stage 3 rates
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Stage 3:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("goldCoinDropRateStage3"), new GUIContent("Gold Coin Rate (minutes)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("silverCoinDropRateStage3"), new GUIContent("Silver Coin Rate (minutes)"));
        
        // Show fallback info
        var goldStage3 = serializedObject.FindProperty("goldCoinDropRateStage3").floatValue;
        var silverStage3 = serializedObject.FindProperty("silverCoinDropRateStage3").floatValue;
        if (goldStage3 <= 0) EditorGUILayout.HelpBox("Will use Stage 1 gold rate as fallback", MessageType.Info);
        if (silverStage3 <= 0) EditorGUILayout.HelpBox("Will use Stage 1 silver rate as fallback", MessageType.Info);
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Other Rates", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("poopRate"), new GUIContent("Poop Rate (minutes)"));
        
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHungerStage1"), new GUIContent("Max Hunger (Stage 1)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHungerStage2"), new GUIContent("Max Hunger (Stage 2)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHungerStage3"), new GUIContent("Max Hunger (Stage 3)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hungerDepleteRate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("areaHappinessRate"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("pokeHappinessValue"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hungerHappinessThreshold"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("hungerHappinessDrainRate"));
        
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
        
        // Use the CORRECT property name from MonsterDataSO
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsIconImg"), new GUIContent("Monster Icons"), true);
        
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
        EditorGUILayout.Space(5);  // Add this line for consistency
    }
}
#endif