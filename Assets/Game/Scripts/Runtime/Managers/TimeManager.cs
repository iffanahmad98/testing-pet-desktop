using UnityEngine;
using Unity.Collections;
using System;
using TMPro;
using System.Collections.Generic;

namespace MagicalGarden.Manager
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager Instance;
        private List<TimedEvent> events = new List<TimedEvent>();
        private float nextCheck = 0;
        public DateTime currentTime;
        public DateTime lastLoginTime;
        public DateTime lastDailyReset;

        [Header("⏱ Real-Time Settings")]
        public bool useSystemTime = true; // true = pakai DateTime.UtcNow
        public TimeSpan utcOffset = TimeSpan.FromHours(7); // Default ke WIB

        [Header("📅 Time Status (Debug Only)")]
        [ReadOnly] public string currentTimeStr;
        [ReadOnly] public string lastLoginTimeStr;
        [ReadOnly] public string lastDailyResetStr;
        public int addDayDebug = 0;
        public TextMeshProUGUI timeText;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(gameObject);
            else
                Instance = this;

            LoadTime();
            UpdateCurrentTime();
            UpdateDebugStrings();
            utcOffset += TimeSpan.FromDays(addDayDebug);
        }
        void Update()
        {
            UpdateCurrentTime();
            UpdateDebugStrings();
            timeText.text = $"{currentTime:dd MMM yyyy - HH:mm:ss}";
            if (Time.time >= nextCheck)
            {
                CheckEvents();
                nextCheck = Time.time + 1f; // Cek setiap 1 detik
            }
        }
#region Handle Event
        void CheckEvents()
        {
            DateTime now = DateTime.Now;

            for (int i = events.Count - 1; i >= 0; i--)
            {
                if (now >= events[i].triggerTime)
                {
                    events[i].callback?.Invoke();
                    events.RemoveAt(i);
                }
            }
        }
        public void AddEvent(DateTime triggerTime, Action callback)
        {
            events.Add(new TimedEvent(triggerTime, callback));
        }
#endregion
        public void UpdateCurrentTime()
        {
            currentTime = DateTime.UtcNow + utcOffset;
        }
        public TimeSpan GetTimeSinceLastLogin()
        {
            return currentTime - lastLoginTime;
        }

        public bool IsNewDay()
        {
            return currentTime.Date > lastDailyReset.Date;
        }

        public void ResetDaily()
        {
            lastDailyReset = currentTime;
            SaveTime();
            // reset tugas harian, claim harian, event, dll.
        }
        public bool IsProductionReady(DateTime lastCollected, TimeSpan productionDuration)
        {
            return (currentTime - lastCollected) >= productionDuration;
        }
        public void SaveTime()
        {
            PlayerPrefs.SetString("lastLoginTime", currentTime.ToBinary().ToString());
            PlayerPrefs.SetString("lastDailyReset", lastDailyReset.ToBinary().ToString());
        }
        public int CalculateProductionOffline(DateTime lastCollected, TimeSpan interval, int amountPerInterval)
        {
            var elapsed = currentTime - lastCollected;
            int totalCycles = (int)(elapsed.TotalSeconds / interval.TotalSeconds);
            return totalCycles * amountPerInterval;
        }
        public void LoadTime()
        {
            if (PlayerPrefs.HasKey("lastLoginTime"))
                lastLoginTime = DateTime.FromBinary(Convert.ToInt64(PlayerPrefs.GetString("lastLoginTime")));
            else
                lastLoginTime = DateTime.UtcNow + utcOffset;

            if (PlayerPrefs.HasKey("lastDailyReset"))
                lastDailyReset = DateTime.FromBinary(Convert.ToInt64(PlayerPrefs.GetString("lastDailyReset")));
            else
                lastDailyReset = DateTime.UtcNow + utcOffset;
        }

        private void UpdateDebugStrings()
        {
            currentTimeStr = currentTime.ToString("yyyy-MM-dd HH:mm:ss");
            lastLoginTimeStr = lastLoginTime.ToString("yyyy-MM-dd HH:mm:ss");
            lastDailyResetStr = lastDailyReset.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
    public class TimedEvent
    {
        public DateTime triggerTime;
        public Action callback;

        public TimedEvent(DateTime time, Action action)
        {
            triggerTime = time;
            callback = action;
        }
    }
}
