using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

public class FacilityManager : MonoBehaviour
{
    [SerializeField] private Button magicShovelButton;
    [SerializeField] private FacilityDatabaseSO facilityDatabase;
    [SerializeField] private GameObject magicShovelPrefab;
    [SerializeField] private GameObject timeKeeperNormalObject;
    [SerializeField] private GameObject timeKeeperAdvanceObject;

    [Header("Pumpkin Facility")]
    [SerializeField] private GameObject pumpkinCarObject;
    [SerializeField] private GameObject pumpkinMiniObject;
    [SerializeField] private PomodoroPhaseManager pomodoroPhaseManager;

    public FacilityDatabaseSO FacilityDatabase => facilityDatabase;

    // Event to notify when a Time Keeper facility state changes
    public event Action OnTimeKeeperStateChanged;

    private Dictionary<string, float> lastUsedTime = new Dictionary<string, float>();
    private Dictionary<string, Coroutine> activeCooldownCoroutines = new Dictionary<string, Coroutine>();

    private void Awake()
    {
        ServiceLocator.Register(this);

        // SaveSystem.TryPurchaseFacility(facilityDatabase.GetFacility("F1"));
        magicShovelButton.onClick.AddListener(() => UseFacility("F1"));
    }

    private void Start()
    {
        // Initialize Pumpkin Facility state based on saved data
        InitializePumpkinFacilityState();
    }

    /// <summary>
    /// Initialize Pumpkin Facility state on game start based on saved data
    /// </summary>
    private void InitializePumpkinFacilityState()
    {
        // Find Pumpkin Facility in database
        var pumpkinFacility = facilityDatabase?.allFacilities?.Find(f => f.isFreeToggleFacility);

        if (pumpkinFacility != null)
        {
            // Check if Pumpkin Facility is owned (applied)
            bool isPumpkinApplied = SaveSystem.IsFacilityOwned(pumpkinFacility.facilityID);

            if (isPumpkinApplied)
            {
                // Enable Pumpkin Mini, disable Pumpkin Car
                if (pumpkinMiniObject != null)
                    pumpkinMiniObject.SetActive(true);
                if (pumpkinCarObject != null)
                    pumpkinCarObject.SetActive(false);

                Debug.Log("Pumpkin Facility initialized as Applied (Pumpkin Mini enabled)");
            }
            else
            {
                // Disable both Pumpkin Mini and Pumpkin Car
                if (pumpkinMiniObject != null)
                    pumpkinMiniObject.SetActive(false);
                if (pumpkinCarObject != null)
                    pumpkinCarObject.SetActive(false);

                Debug.Log("Pumpkin Facility initialized as Unapplied (both disabled)");
            }
        }
    }

    public FacilityDataSO GetFacilityByID(string facilityID)
    {
        return facilityDatabase.GetFacility(facilityID);
    }

    public List<FacilityDataSO> GetAllFacilities() => facilityDatabase.allFacilities;

    public bool CanUseFacility(string facilityID)
    {
        // if (!SaveSystem.PlayerConfig.HasFacility(facilityID))
        // {
        //     Debug.LogWarning($"Facility {facilityID} is not owned.");
        //     return false;
        // }

        var facility = GetFacilityByID(facilityID);
        if (facility == null) return false;

        // Check if this is a Time Keeper facility and if the other Time Keeper is on cooldown
        if (IsTimeKeeperFacility(facilityID))
        {
            string otherTimeKeeperID = GetOtherTimeKeeperID(facilityID);
            if (!string.IsNullOrEmpty(otherTimeKeeperID) && IsOnCooldown(otherTimeKeeperID))
            {
                Debug.LogWarning($"Cannot use {facilityID} while {otherTimeKeeperID} is on cooldown.");
                return false;
            }
        }

        if (!lastUsedTime.ContainsKey(facilityID))
            return true;

        float elapsed = Time.time - lastUsedTime[facilityID];
        return elapsed >= facility.cooldownSeconds;
    }

    public bool UseFacility(string facilityID)
    {
        if (!CanUseFacility(facilityID)) return false;

        var facility = GetFacilityByID(facilityID);
        if (facility == null) return false;

        lastUsedTime[facilityID] = Time.time;

        switch (facility.facilityID)
        {
            case "F1":
                StartCoroutine(UseMagicShovel());
                break;

            case "F2": // Time Keeper Pro (Advance)
                if (activeCooldownCoroutines.ContainsKey(facilityID))
                {
                    StopCoroutine(activeCooldownCoroutines[facilityID]);
                }
                activeCooldownCoroutines[facilityID] = StartCoroutine(HandleTimeKeeperCooldown(facilityID, timeKeeperAdvanceObject));

                // Apply time skip effect: +24 hours for evolution and coins
                var monsterManagerAdvance = ServiceLocator.Get<MonsterManager>();
                if (monsterManagerAdvance != null)
                {
                    monsterManagerAdvance.SimulateTimeSkip(24f); // 24 hours
                }

                OnTimeKeeperStateChanged?.Invoke(); // Notify state change
                break;

            case "F3": // Time Keeper Normal
                if (activeCooldownCoroutines.ContainsKey(facilityID))
                {
                    StopCoroutine(activeCooldownCoroutines[facilityID]);
                }
                activeCooldownCoroutines[facilityID] = StartCoroutine(HandleTimeKeeperCooldown(facilityID, timeKeeperNormalObject));

                // Apply time skip effect: +6 hours for evolution and coins
                var monsterManager = ServiceLocator.Get<MonsterManager>();
                if (monsterManager != null)
                {
                    monsterManager.SimulateTimeSkip(6f); // 6 hours
                }

                OnTimeKeeperStateChanged?.Invoke(); // Notify state change
                break;

            // Add more cases like:
            // case "HealingStation":
            //     StartCoroutine(UseHealingStation());
            //     break;

            default:
                Debug.LogWarning($"Facility '{facility.facilityID}' not implemented.");
                break;
        }

        return true;
    }

    private IEnumerator UseMagicShovel()
    {
        var monsterManager = ServiceLocator.Get<MonsterManager>();
        var poops = new List<PoopController>(monsterManager.activePoops);
        var magicShovelAnim = magicShovelPrefab.GetComponent<UIAnimator>();
        var magicShovelRT = magicShovelPrefab.GetComponent<RectTransform>();

        foreach (var poop in poops)
        {
            if (poop != null && poop.gameObject.activeInHierarchy)
            {
                magicShovelRT.position = poop.transform.position;
                magicShovelPrefab.SetActive(true);
                magicShovelAnim.Play();
                yield return new WaitForSeconds(0.5f); // Wait for animation to play
                poop.OnCollected(); // or monsterManager.DespawnToPool(poop.gameObject)
                magicShovelPrefab.SetActive(false);
                yield return new WaitForSeconds(0.2f); // Add a delay for effect
            }
        }

        Debug.Log("Magic Shovel used to clean all poop in the current area.");
    }

    private IEnumerator HandleTimeKeeperCooldown(string facilityID, GameObject timeKeeperObject)
    {
        // Show the Time Keeper object
        if (timeKeeperObject != null)
        {
            timeKeeperObject.SetActive(true);
            Debug.Log($"Time Keeper {facilityID} is now visible during cooldown.");
        }

        // Get cooldown duration
        var facility = GetFacilityByID(facilityID);
        float cooldownDuration = facility != null ? facility.cooldownSeconds : 10f;

        // Wait for cooldown to finish
        yield return new WaitForSeconds(cooldownDuration);

        // Hide the Time Keeper object
        if (timeKeeperObject != null)
        {
            timeKeeperObject.SetActive(false);
            Debug.Log($"Time Keeper {facilityID} cooldown finished, hiding object.");
        }

        // Remove coroutine reference
        if (activeCooldownCoroutines.ContainsKey(facilityID))
        {
            activeCooldownCoroutines.Remove(facilityID);
        }

        // Notify that state changed when cooldown finished
        OnTimeKeeperStateChanged?.Invoke();
    }

    public float GetCooldownRemaining(string facilityID)
    {
        var facility = GetFacilityByID(facilityID);
        if (facility == null) return 0f;

        if (!lastUsedTime.ContainsKey(facilityID))
            return 0f;

        float elapsed = Time.time - lastUsedTime[facilityID];
        float remaining = facility.cooldownSeconds - elapsed;
        return Mathf.Max(0f, remaining);
    }

    public void CancelFacilityCooldown(string facilityID)
    {
        if (lastUsedTime.ContainsKey(facilityID))
        {
            lastUsedTime.Remove(facilityID);

            // Stop active cooldown coroutine if exists
            if (activeCooldownCoroutines.ContainsKey(facilityID))
            {
                if (activeCooldownCoroutines[facilityID] != null)
                {
                    StopCoroutine(activeCooldownCoroutines[facilityID]);
                }
                activeCooldownCoroutines.Remove(facilityID);
            }

            // Hide Time Keeper objects when cooldown is cancelled
            if (facilityID == "F2" && timeKeeperAdvanceObject != null)
            {
                timeKeeperAdvanceObject.SetActive(false);
                OnTimeKeeperStateChanged?.Invoke(); // Notify state change
            }
            else if (facilityID == "F3" && timeKeeperNormalObject != null)
            {
                timeKeeperNormalObject.SetActive(false);
                OnTimeKeeperStateChanged?.Invoke(); // Notify state change
            }

            Debug.Log($"Cooldown cancelled for facility: {facilityID}");
        }
    }

    // Helper method to check if a facility is a Time Keeper
    private bool IsTimeKeeperFacility(string facilityID)
    {
        return facilityID == "F2" || facilityID == "F3";
    }

    // Helper method to get the other Time Keeper ID
    private string GetOtherTimeKeeperID(string facilityID)
    {
        if (facilityID == "F2") return "F3"; // If Advance, return Normal
        if (facilityID == "F3") return "F2"; // If Normal, return Advance
        return null;
    }

    // Helper method to check if a facility is on cooldown
    private bool IsOnCooldown(string facilityID)
    {
        if (!lastUsedTime.ContainsKey(facilityID))
            return false;

        var facility = GetFacilityByID(facilityID);
        if (facility == null) return false;

        float elapsed = Time.time - lastUsedTime[facilityID];
        return elapsed < facility.cooldownSeconds;
    }

    // Public method to check if this specific facility is on its own cooldown (not blocked by another)
    public bool IsOnOwnCooldown(string facilityID)
    {
        return IsOnCooldown(facilityID);
    }

    /// <summary>
    /// Apply Pumpkin Facility - Enable Pumpkin Mini, Disable Pumpkin Car
    /// </summary>
    public void ApplyPumpkinFacility()
    {
        // Enable Pumpkin Mini
        if (pumpkinMiniObject != null)
        {
            pumpkinMiniObject.SetActive(true);
            Debug.Log("Pumpkin Mini enabled");
        }
        else
        {
            Debug.LogWarning("Pumpkin Mini object is not assigned in FacilityManager");
        }

        // Disable Pumpkin Car
        if (pumpkinCarObject != null)
        {
            pumpkinCarObject.SetActive(false);
            Debug.Log("Pumpkin Car disabled");
        }
        else
        {
            Debug.LogWarning("Pumpkin Car object is not assigned in FacilityManager");
        }
    }

    /// <summary>
    /// Unapply Pumpkin Facility - Disable both Pumpkin Mini and Pumpkin Car, Reset Pomodoro Phase
    /// </summary>
    public void UnapplyPumpkinFacility()
    {
        // Disable Pumpkin Mini
        if (pumpkinMiniObject != null)
        {
            pumpkinMiniObject.SetActive(false);
            Debug.Log("Pumpkin Mini disabled");
        }

        // Disable Pumpkin Car
        if (pumpkinCarObject != null)
        {
            pumpkinCarObject.SetActive(false);
            Debug.Log("Pumpkin Car disabled");
        }

        // Reset Pomodoro phase time execution
        if (pomodoroPhaseManager != null)
        {
            pomodoroPhaseManager.StopTimer();
            Debug.Log("Pomodoro phase time execution reset");
        }
        else
        {
            Debug.LogWarning("Pomodoro Phase Manager is not assigned in FacilityManager");
        }
    }
}
