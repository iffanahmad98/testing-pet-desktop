using UnityEngine;

internal sealed class PlayerPrefsTutorialProgressStore : ITutorialProgressStore
{
    private readonly string _keyPrefix;

    public PlayerPrefsTutorialProgressStore(string keyPrefix)
    {
        _keyPrefix = string.IsNullOrEmpty(keyPrefix) ? "tutorial_" : keyPrefix;
    }

    private string GetKey(int stepIndex)
    {
        return _keyPrefix + stepIndex;
    }

    public bool IsCompleted(int stepIndex)
    {
        if (stepIndex < 0)
            return true;

        var key = GetKey(stepIndex);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public void MarkCompleted(int stepIndex)
    {
        if (stepIndex < 0)
            return;

        var key = GetKey(stepIndex);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public void ClearAll(int stepCount)
    {
        if (stepCount <= 0)
            return;

        for (int i = 0; i < stepCount; i++)
        {
            var key = GetKey(i);
            PlayerPrefs.DeleteKey(key);
        }
    }
}