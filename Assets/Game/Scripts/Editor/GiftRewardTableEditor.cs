using UnityEngine;
using UnityEditor;
using MagicalGarden.Gift;
using MagicalGarden.Inventory;

#if UNITY_EDITOR
[CustomEditor(typeof(GiftRewardTable))]
public class GiftRewardTableEditor : Editor
{
    private ItemData feedItem;
    private ItemData medicineItem;
    private ItemData goldenTicketItem;
    private ItemData decorationItem;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GiftRewardTable table = (GiftRewardTable)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Quick Setup", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Use buttons below to quickly populate reward list with preset values.\n" +
            "Assign ItemData references first for item-based rewards.",
            MessageType.Info);

        EditorGUILayout.Space(5);

        // Item references
        EditorGUILayout.LabelField("Item References (Optional)", EditorStyles.boldLabel);
        feedItem = (ItemData)EditorGUILayout.ObjectField("Feed Item", feedItem, typeof(ItemData), false);
        medicineItem = (ItemData)EditorGUILayout.ObjectField("Medicine Item", medicineItem, typeof(ItemData), false);
        goldenTicketItem = (ItemData)EditorGUILayout.ObjectField("Golden Ticket", goldenTicketItem, typeof(ItemData), false);
        decorationItem = (ItemData)EditorGUILayout.ObjectField("Decoration Item", decorationItem, typeof(ItemData), false);

        EditorGUILayout.Space(10);

        // Setup buttons
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Setup Small Gift", GUILayout.Height(30)))
        {
            SetupSmallGift(table);
        }

        if (GUILayout.Button("Setup Medium Gift", GUILayout.Height(30)))
        {
            SetupMediumGift(table);
        }

        if (GUILayout.Button("Setup Large Gift", GUILayout.Height(30)))
        {
            SetupLargeGift(table);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Validation
        if (GUILayout.Button("Validate Probabilities", GUILayout.Height(25)))
        {
            ValidateProbabilities(table);
        }

        EditorGUILayout.Space(5);

        // Show total probability
        float totalProb = table.GetTotalProbability();
        Color prevColor = GUI.color;
        GUI.color = Mathf.Abs(totalProb - 100f) < 0.01f ? Color.green : Color.yellow;
        EditorGUILayout.HelpBox($"Total Probability: {totalProb:F2}%", MessageType.None);
        GUI.color = prevColor;
    }

    private void SetupSmallGift(GiftRewardTable table)
    {
        Undo.RecordObject(table, "Setup Small Gift");
        table.giftSize = GiftSize.Small;
        table.rewards = GiftRewardPresets.CreateSmallGiftRewards(
            feedItem, medicineItem, goldenTicketItem, decorationItem);
        EditorUtility.SetDirty(table);
        Debug.Log("Small Gift rewards setup complete!");
    }

    private void SetupMediumGift(GiftRewardTable table)
    {
        Undo.RecordObject(table, "Setup Medium Gift");
        table.giftSize = GiftSize.Medium;
        table.rewards = GiftRewardPresets.CreateMediumGiftRewards(
            feedItem, medicineItem, goldenTicketItem, decorationItem);
        EditorUtility.SetDirty(table);
        Debug.Log("Medium Gift rewards setup complete!");
    }

    private void SetupLargeGift(GiftRewardTable table)
    {
        Undo.RecordObject(table, "Setup Large Gift");
        table.giftSize = GiftSize.Large;
        table.rewards = GiftRewardPresets.CreateLargeGiftRewards(
            feedItem, medicineItem, goldenTicketItem, decorationItem);
        EditorUtility.SetDirty(table);
        Debug.Log("Large Gift rewards setup complete!");
    }

    private void ValidateProbabilities(GiftRewardTable table)
    {
        float total = table.GetTotalProbability();

        if (Mathf.Abs(total - 100f) < 0.01f)
        {
            EditorUtility.DisplayDialog("Validation Success",
                $"Total probability is {total:F2}% - Perfect!",
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Validation Warning",
                $"Total probability is {total:F2}%\nShould be 100%\n\nDifference: {(total - 100f):F2}%",
                "OK");
        }
    }
}
#endif
