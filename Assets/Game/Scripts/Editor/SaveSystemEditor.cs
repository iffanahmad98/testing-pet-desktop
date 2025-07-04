using UnityEngine;
using UnityEditor;

public class SaveSystemEditor : Editor
{
    [MenuItem("Tools/Save System/Reset Save Data")]
    public static void ResetSaveData()
    {
        if (EditorUtility.DisplayDialog("Reset Save Data", 
            "Are you sure you want to reset all save data? This action cannot be undone.", 
            "Reset", "Cancel"))
        {
            SaveSystem.DeleteAllSaveData();
            SaveSystem.ResetSaveData();
            Debug.Log("Save data has been reset successfully.");
        }
    }
}
