using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using UnityEngine.UI;

public class FacilityManager : MonoBehaviour
{
    public GameObject magicShovelPrefab;
    [SerializeField] private Button magicShovelButton;
    [SerializeField] private FacilityDatabaseSO facilityDatabase;

    public FacilityDatabaseSO FacilityDatabase => facilityDatabase;

    private Dictionary<string, float> lastUsedTime = new Dictionary<string, float>();

    private void Awake()
    {
        ServiceLocator.Register(this);
        SaveSystem.TryPurchaseFacility(facilityDatabase.GetFacility("F1"));
        magicShovelButton.onClick.AddListener(() => UseFacility("F1"));
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

        if (!lastUsedTime.ContainsKey(facilityID))
            return true;

        float elapsed = Time.time - lastUsedTime[facilityID];
        Debug.Log(elapsed >= facility.cooldownSeconds);
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

        foreach (var poop in poops)
        {
            if (poop != null && poop.gameObject.activeInHierarchy)
            {
                magicShovelPrefab.SetActive(true);
                magicShovelPrefab.transform.position = poop.GetComponent<RectTransform>().position;
                magicShovelPrefab.GetComponent<UIAnimator>().Play();
                yield return new WaitForSeconds(1f); // Add a delay for effect
                poop.OnCollected(); // or monsterManager.DespawnToPool(poop.gameObject));
                magicShovelPrefab.SetActive(false);
                yield return new WaitForSeconds(0.5f); // Delay between each poop collection
            }
        }

        Debug.Log("Magic Shovel used to clean all poop in the current area.");
    }
}
