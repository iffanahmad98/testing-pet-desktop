using System;
using System.Collections.Generic;

[Serializable]
public class PlayerConfig
{
    // Player stats
    public int coins = 100;
    public int poop = 0;

    // Time tracking
    public DateTime lastLoginTime;
    public TimeSpan totalPlayTime;

    // Monster collection
    public List<string> monsterIDs = new List<string>();
    public Dictionary<string, MonsterSaveData> monsters = new Dictionary<string, MonsterSaveData>();
    public SettingsData settings = new SettingsData();

    public bool notificationsEnabled = true;
}
[Serializable]
public class SettingsData
{
    public float gameAreaWidth = 1920f;
    public float gameAreaX = 0f;
    public float gameAreaY = 0f;
    public float uiScale = 1f;
    public int languageIndex = 0;
    public int screenState = 0;

    public float masterVolume = 1f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
}