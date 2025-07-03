#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonsterDataSO))]
public class MonsterDataSOEditor : Editor
{
    // Organize foldouts by category and set sensible defaults
    private bool showBasicInfo = false;
    private bool showStats = false;
    private bool showBehavior = false;
    private bool showEvolution = false;
    private bool showAnimations = false;
    private bool showAudio = false;
    private bool showVisuals = false;
    
    // NPC Mode toggle
    private bool isNPCMode = false;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Force the toggle to be visible with a more direct GUI approach
        EditorGUILayout.Space(5);
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("MODE:", GUILayout.Width(50));
        isNPCMode = EditorGUILayout.ToggleLeft("NPC Monster", isNPCMode, EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(5);
        
        // Draw a separator line
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        // Draw appropriate sections based on mode
        if (isNPCMode)
        {
            DrawNPCModeEditor();
        }
        else
        {
            // Regular pet monster editor
            DrawBasicInfo();
            DrawStats();
            DrawBehaviorSettings();
            DrawEvolutionSettings();
            DrawAnimationSettings();
            DrawVisualSettings();
            DrawAudioSettings();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawNPCModeEditor()
    {
        // Draw a header to indicate NPC Mode is active
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        GUILayout.FlexibleSpace();
        EditorGUILayout.LabelField("NPC MONSTER PROPERTIES", EditorStyles.boldLabel);
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(5);
        
        // Draw Basic Info section
        DrawNPCBasicInfo();
        
        // Draw Visual Settings (simplified)
        DrawNPCVisualSettings();
        
        // Draw Animation Settings
        DrawAnimationSettings(); // Reuse standard animation settings
        
        // Draw Behavior Settings (simplified)
        DrawNPCBehaviorSettings();
        
        // Draw Evolution Settings (simplified)
        DrawNPCEvolutionSettings();
        
        // Draw Audio Settings (simplified)
        DrawNPCAudioSettings();
    }

    private void DrawNPCBasicInfo()
    {
        showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "Basic Info", true);
        if (!showBasicInfo) return;

        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterName"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
        
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Classification", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("monType"));
        
        // NPC movement speed - important for NPC behavior
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Movement", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpd"), new GUIContent("Move Speed"));
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawNPCVisualSettings()
    {
        showVisuals = EditorGUILayout.Foldout(showVisuals, "Visual Assets", true);
        if (!showVisuals) return;

        EditorGUI.indentLevel++;
        
        // Icons for NPC display
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Icon Categories", EditorStyles.boldLabel);
        
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Detail Icons:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DetailsIcon"), new GUIContent("Details Icons"));
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Card Icons:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CardIcon"), new GUIContent("Card Icons"));
        EditorGUI.indentLevel--;

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawNPCBehaviorSettings()
    {
        showBehavior = EditorGUILayout.Foldout(showBehavior, "Behavior Settings", true);
        if (!showBehavior) return;

        EditorGUI.indentLevel++;
        
        // Only show behavior configs for NPCs
        EditorGUILayout.PropertyField(serializedObject.FindProperty("evolutionBehaviors"), true);
        
        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawNPCEvolutionSettings()
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("evolutionLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToEvolveStage1"), 
                new GUIContent("Time to Evolve Stage 2 (days)", "For NPCs, this is the time-based evolution trigger"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToEvolveStage2"), 
                new GUIContent("Time to Evolve Stage 3 (days)", "For NPCs, this is the time-based evolution trigger"));

            // For NPCs we only show time-based evolution requirements
            EditorGUILayout.HelpBox("NPC evolution uses time-based evolution only.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("Evolution is disabled for this NPC monster.", MessageType.Info);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
    }

    private void DrawNPCAudioSettings()
    {
        showAudio = EditorGUILayout.Foldout(showAudio, "Audio Settings", true);
        if (!showAudio) return;

        EditorGUI.indentLevel++;
        
        // Idle sounds (random ambient sounds)
        EditorGUILayout.PropertyField(serializedObject.FindProperty("idleSounds"), true);

        EditorGUILayout.Space(3);
        
        // Only include relevant sound events for NPCs
        EditorGUILayout.LabelField("Event Sounds", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("interactionSound"), 
            new GUIContent("Interaction Sound"));
        
        // Evolution sounds
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Special Event Sounds", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("evolveSound"), 
            new GUIContent("Evolve Sound"));

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(5);
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealthStage1"), new GUIContent("Max Health (Stage 1)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealthStage2"), new GUIContent("Max Health (Stage 2)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxHealthStage3"), new GUIContent("Max Health (Stage 3)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpd"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("foodDetectionRange"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("eatDistance"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFullnessStage1"), new GUIContent("Max Fullness (Stage 1)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFullnessStage2"), new GUIContent("Max Fullness (Stage 2)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxFullnessStage3"), new GUIContent("Max Fullness (Stage 3)"));

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Base Values", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseHunger"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("baseHappiness"));

        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Drop Rates", EditorStyles.boldLabel);

        // Stage 1 rates (always shown)
        EditorGUILayout.LabelField("Stage 1:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("goldCoinDropRateStage1"), new GUIContent("Gold Coin Rate (minutes)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("platCoinDropRateStage1"), new GUIContent("Platinum Coin Rate (minutes)")); // Changed from Silver to Platinum
        EditorGUI.indentLevel--;

        // Stage 2 rates
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Stage 2:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("goldCoinDropRateStage2"), new GUIContent("Gold Coin Rate (minutes)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("platCoinDropRateStage2"), new GUIContent("Platinum Coin Rate (minutes)")); // Changed from Silver to Platinum

        // Show fallback info
        var goldStage2 = serializedObject.FindProperty("goldCoinDropRateStage2").floatValue;
        var silverStage2 = serializedObject.FindProperty("platCoinDropRateStage2").floatValue;
        if (goldStage2 <= 0) EditorGUILayout.HelpBox("Will use Stage 1 gold rate as fallback", MessageType.Info);
        if (silverStage2 <= 0) EditorGUILayout.HelpBox("Will use Stage 1 silver rate as fallback", MessageType.Info);
        EditorGUI.indentLevel--;

        // Stage 3 rates
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Stage 3:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("goldCoinDropRateStage3"), new GUIContent("Gold Coin Rate (minutes)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("platCoinDropRateStage3"), new GUIContent("Platinum Coin Rate (minutes)"));

        // Show fallback info
        var goldStage3 = serializedObject.FindProperty("goldCoinDropRateStage3").floatValue;
        var silverStage3 = serializedObject.FindProperty("platCoinDropRateStage3").floatValue;
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxNutritionStage1"), new GUIContent("Max Hunger (Stage 1)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxNutritionStage2"), new GUIContent("Max Hunger (Stage 2)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxNutritionStage3"), new GUIContent("Max Hunger (Stage 3)"));
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
            EditorGUILayout.PropertyField(serializedObject.FindProperty("evolutionLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToEvolveStage1"), new GUIContent("Time to Evolve Stage 2 (days)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToEvolveStage2"), new GUIContent("Time to Evolve Stage 3 (days)"));

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

        // Add header for better organization
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Icon Categories", EditorStyles.boldLabel);
        
        // Detail icons section
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Detail Icons:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("DetailsIcon"), new GUIContent("Details Icons", "Icons used in detail panels and information screens"));
        EditorGUI.indentLevel--;
        
        // Catalogue icons section
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Catalogue Icons:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CatalogueIcon"), new GUIContent("Catalogue Icons", "Icons used in listings and catalogue views"));
        EditorGUI.indentLevel--;

        // Keep existing monster icons for backward compatibility
        EditorGUILayout.Space(2);
        EditorGUILayout.LabelField("Card Icons:", EditorStyles.miniBoldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CardIcon"), new GUIContent("Card Icons", "General purpose monster icons"));
        EditorGUI.indentLevel--;

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