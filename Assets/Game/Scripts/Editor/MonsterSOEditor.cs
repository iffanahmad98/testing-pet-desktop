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

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Get the monster type to determine which editor to show
        var monsterTypeProperty = serializedObject.FindProperty("monType");
        var currentMonsterType = (MonsterType)monsterTypeProperty.enumValueIndex;
        bool isNPCType = currentMonsterType == MonsterType.NPC;

        // Show monster type selection prominently at the top with better styling
        EditorGUILayout.Space(8);
        
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.FlexibleSpace();
            
            // Add icon or color indicator for better visual feedback
            if (isNPCType)
            {
                var prevColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.7f, 0.9f, 1f, 1f); // Light blue for NPC
                EditorGUILayout.LabelField("🤖 MONSTER TYPE:", EditorStyles.boldLabel, GUILayout.Width(140));
                GUI.backgroundColor = prevColor;
            }
            else
            {
                EditorGUILayout.LabelField("🐾 MONSTER TYPE:", EditorStyles.boldLabel, GUILayout.Width(140));
            }
            
            EditorGUILayout.PropertyField(monsterTypeProperty, GUIContent.none, GUILayout.Width(120));
            GUILayout.FlexibleSpace();
        }
        
        EditorGUILayout.Space(10);

        // Add visual separator
        var rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
        EditorGUILayout.Space(8);

        // Draw appropriate sections based on monster type
        if (isNPCType)
        {
            DrawNPCModeEditor();
        }
        else
        {
            DrawRegularModeEditor();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawNPCModeEditor()
    {
        // Add a subtle header for NPC mode
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            var headerStyle = new GUIStyle(EditorStyles.largeLabel);
            headerStyle.normal.textColor = new Color(0.4f, 0.7f, 1f, 1f);
            EditorGUILayout.LabelField("NPC CONFIGURATION", headerStyle);
            GUILayout.FlexibleSpace();
        }
        
        EditorGUILayout.Space(8);
        
        DrawNPCBasicInfo();
        DrawNPCVisualSettings();
        DrawAnimationSettings();
        DrawNPCBehaviorSettings();
        DrawNPCEvolutionSettings();
        DrawNPCAudioSettings();
    }

    private void DrawRegularModeEditor()
    {
        // Add a subtle header for regular mode
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.FlexibleSpace();
            var headerStyle = new GUIStyle(EditorStyles.largeLabel);
            headerStyle.normal.textColor = new Color(0.4f, 0.8f, 0.4f, 1f);
            EditorGUILayout.LabelField("PET MONSTER CONFIGURATION", headerStyle);
            GUILayout.FlexibleSpace();
        }
        
        EditorGUILayout.Space(8);
        
        DrawBasicInfo();
        DrawStats();
        DrawBehaviorSettings();
        DrawEvolutionSettings();
        DrawAnimationSettings();
        DrawVisualSettings();
        DrawAudioSettings();
    }

    private void DrawNPCBasicInfo()
    {
        using (var scope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showBasicInfo = EditorGUILayout.Foldout(showBasicInfo, "📋 Basic Information", true, EditorStyles.foldoutHeader);
            if (!showBasicInfo) return;

            EditorGUILayout.Space(5);
            
            using (new EditorGUI.IndentLevelScope())
            {
                // Identity section with better grouping
                DrawSectionHeader("Identity");
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterName"), new GUIContent("Name"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("id"), new GUIContent("ID"));
                }
                
                EditorGUILayout.Space(8);
                
                // Classification section
                DrawSectionHeader("Classification");
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("monType"), new GUIContent("Type"));
                    }
                }
                
                EditorGUILayout.Space(8);

                // Movement section
                DrawSectionHeader("Movement");
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpd"), new GUIContent("Move Speed"));
                }

                EditorGUILayout.Space(8);

                // Purchase Requirements section
                DrawSectionHeader("Purchase Requirements");
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterPrice"), new GUIContent("Price"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("prerequisiteNPCId"), new GUIContent("Prerequisite NPC ID", "NPC yang harus dimiliki player sebelum bisa membeli NPC ini"));

                    // Show helpful info if prerequisite is set
                    var prerequisiteId = serializedObject.FindProperty("prerequisiteNPCId").stringValue;
                    if (!string.IsNullOrEmpty(prerequisiteId))
                    {
                        EditorGUILayout.HelpBox($"🔒 Requires player to own: {prerequisiteId}", MessageType.Info);
                    }
                }
            }

            EditorGUILayout.Space(5);
        }

        EditorGUILayout.Space(5);
    }

    private void DrawNPCVisualSettings()
    {
        using (var scope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showVisuals = EditorGUILayout.Foldout(showVisuals, "🎨 Visual Assets", true, EditorStyles.foldoutHeader);
            if (!showVisuals) return;

            EditorGUILayout.Space(5);
            
            using (new EditorGUI.IndentLevelScope())
            {
                DrawSectionHeader("Icon Categories");
                
                // Detail Icons
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Detail Icons", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("DetailsIcon"), GUIContent.none);
                }
                
                EditorGUILayout.Space(3);
                
                // Card Icons
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Card Icons", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("CardIcon"), GUIContent.none);
                }
            }
            
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.Space(5);
    }

    private void DrawNPCEvolutionSettings()
    {
        using (var scope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showEvolution = EditorGUILayout.Foldout(showEvolution, "🔄 Evolution System", true, EditorStyles.foldoutHeader);
            if (!showEvolution) return;

            EditorGUILayout.Space(5);
            
            using (new EditorGUI.IndentLevelScope())
            {
                DrawSectionHeader("Basic Settings");
                var canEvolve = serializedObject.FindProperty("canEvolve").boolValue;
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("canEvolve"));

                    if (canEvolve)
                    {
                        EditorGUILayout.Space(3);
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("evolutionLevel"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToEvolveStage1"), 
                            new GUIContent("Days to Stage 2", "Time-based evolution trigger for NPCs"));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty("timeToEvolveStage2"), 
                            new GUIContent("Days to Stage 3", "Time-based evolution trigger for NPCs"));
                    }
                }

                EditorGUILayout.Space(5);
                
                if (canEvolve)
                {
                    EditorGUILayout.HelpBox("💡 NPC evolution uses time-based triggers only.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("❌ Evolution is disabled for this NPC monster.", MessageType.Info);
                }
            }
            
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.Space(5);
    }

    private void DrawNPCBehaviorSettings()
    {
        using (var scope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showBehavior = EditorGUILayout.Foldout(showBehavior, "⚙️ Behavior Settings", true, EditorStyles.foldoutHeader);
            if (!showBehavior) return;

            EditorGUILayout.Space(5);
            
            using (new EditorGUI.IndentLevelScope())
            {
                DrawSectionHeader("Behavior Configurations");
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("evolutionBehaviors"), true);
                }
            }
            
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.Space(5);
    }

    private void DrawNPCAudioSettings()
    {
        using (var scope = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            showAudio = EditorGUILayout.Foldout(showAudio, "🔊 Audio Settings", true, EditorStyles.foldoutHeader);
            if (!showAudio) return;

            EditorGUILayout.Space(5);
            
            using (new EditorGUI.IndentLevelScope())
            {
                // Ambient Sounds
                DrawSectionHeader("Ambient Sounds");
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("idleSounds"), new GUIContent("Idle Sounds"), true);
                }

                EditorGUILayout.Space(5);
                
                // Event Sounds
                DrawSectionHeader("Event Sounds");
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("interactionSound"), 
                        new GUIContent("Interaction Sound"));
                }
                
                EditorGUILayout.Space(5);
                
                // Special Sounds
                DrawSectionHeader("Special Events");
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("evolveSound"), 
                        new GUIContent("Evolution Sound"));
                }
            }
            
            EditorGUILayout.Space(5);
        }
        
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
        EditorGUILayout.PropertyField(serializedObject.FindProperty("poopType"));

        // Only show pricing & gacha for non-NPC monsters
        var monsterTypeProperty = serializedObject.FindProperty("monType");
        var currentMonsterType = (MonsterType)monsterTypeProperty.enumValueIndex;
        if (currentMonsterType != MonsterType.NPC)
        {
            // Pricing Section
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Pricing & Gacha", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("monsterPrice"), new GUIContent("Buy Price"));

            // Sell prices
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sellPriceStage1"), new GUIContent("Sell Price (Stage 1)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sellPriceStage2"), new GUIContent("Sell Price (Stage 2)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sellPriceStage3"), new GUIContent("Sell Price (Stage 3)"));

            // Gacha data
            EditorGUILayout.Space(2);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gachaChancePercent"), new GUIContent("Gacha Chance (Decimal)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("gachaChanceDisplay"), new GUIContent("Gacha Chance (Display)"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("isGachaOnly"), new GUIContent("Gacha Only"));
        }
        else
        {
            EditorGUILayout.HelpBox("Pricing and Gacha settings are not applicable for NPC monsters.", MessageType.Info);
        }

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
        
        // Show Nutrition fields in Stats section
        EditorGUILayout.Space(3);
        EditorGUILayout.LabelField("Nutrition Values", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxNutritionStage1"), new GUIContent("Max Nutrition (Stage 1)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxNutritionStage2"), new GUIContent("Max Nutrition (Stage 2)"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("maxNutritionStage3"), new GUIContent("Max Nutrition (Stage 3)"));
        
        EditorGUILayout.PropertyField(serializedObject.FindProperty("moveSpd"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("foodDetectionRange"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("eatDistance"));

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

            // Add time-based evolution settings for regular monsters too
            EditorGUILayout.Space(3);
            EditorGUILayout.LabelField("Time-Based Evolution", EditorStyles.boldLabel);
            
            // Check if the properties exist before trying to display them
            var timeToEvolveStage1Prop = serializedObject.FindProperty("timeToEvolveStage1");
            var timeToEvolveStage2Prop = serializedObject.FindProperty("timeToEvolveStage2");
            
            if (timeToEvolveStage1Prop != null)
            {
                EditorGUILayout.PropertyField(timeToEvolveStage1Prop, 
                    new GUIContent("Days to Stage 2", "Time required to evolve to stage 2"));
            }
            else
            {
                EditorGUILayout.HelpBox("timeToEvolveStage1 property not found in MonsterDataSO", MessageType.Warning);
            }
            
            if (timeToEvolveStage2Prop != null)
            {
                EditorGUILayout.PropertyField(timeToEvolveStage2Prop, 
                    new GUIContent("Days to Stage 3", "Time required to evolve to stage 3"));
            }
            else
            {
                EditorGUILayout.HelpBox("timeToEvolveStage2 property not found in MonsterDataSO", MessageType.Warning);
            }

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

    // Helper method for consistent section headers
    private void DrawSectionHeader(string title)
    {
        var headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.normal.textColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        EditorGUILayout.LabelField(title, headerStyle);
        EditorGUILayout.Space(2);
    }
}
#endif