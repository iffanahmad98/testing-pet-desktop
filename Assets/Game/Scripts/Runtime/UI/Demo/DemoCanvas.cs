using UnityEngine;
using UnityEngine.UI;
using System;

public class DemoCanvas : MonoBehaviour
{
    [Header ("Build Settings")]
    [SerializeField] bool isDemo = false;

    [Header("UI")]
    [SerializeField] Image demoPanel;

    [Header("Data")]
    PlayerConfig playerConfig;

    void Start()
    {
        if (isDemo) {
            playerConfig = SaveSystem.PlayerConfig;
            Invoke("nStart", 0.5f);
        }
    }

    void nStart()
    {
        playerConfig.SaveFirstLoginTime ();
        CheckExpiredDemoDay();
    }

    void CheckExpiredDemoDay()
    {
        DateTime firstLogin = playerConfig.firstLoginTime;

        double daysPassed = (DateTime.Now - firstLogin).TotalDays;

        if (daysPassed >= 14)
        {
            demoPanel.gameObject.SetActive(true);
        }
        else
        {
            demoPanel.gameObject.SetActive(false);
        }
    }
}