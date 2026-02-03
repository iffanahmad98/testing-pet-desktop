using UnityEngine;

internal sealed class PlayerPrefsTutorialProgressStore : ITutorialProgressStore
{
    private readonly string _keyPrefix;

    public PlayerPrefsTutorialProgressStore(string keyPrefix)
    {
        _keyPrefix = string.IsNullOrEmpty(keyPrefix) ? "tutorial_" : keyPrefix;
    }

    private string GetKey(string tutorialId)
    {
        return _keyPrefix + tutorialId;
    }

    public bool IsCompleted(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return true;

        var key = GetKey(tutorialId);
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public void MarkCompleted(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return;

        var key = GetKey(tutorialId);
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
    }

    public void Clear(string tutorialId)
    {
        if (string.IsNullOrEmpty(tutorialId))
            return;

        var key = GetKey(tutorialId);
        PlayerPrefs.DeleteKey(key);
    }
}