using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class MonsterCollectionItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private string monsterId;
    [SerializeField] private bool isUnlocked = false;
    [SerializeField] private int maxEvolutionLevel = 1;
    [SerializeField] private List<int> unlockedEvolutions = new List<int>();
    [SerializeField] private int monsterCount = 0;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] MonsterCollectionUI monsterCollectionUI;

    // 1D array - each element represents one evolution level
    public GameObject[] evolutionContainers; // Each container has locked/unlocked children

    private void Awake()
    {
        // Initialize as locked by default
        isUnlocked = false;
        canvasGroup = GetComponent<CanvasGroup>();
        monsterCollectionUI = transform.GetComponentInParent<MonsterCollectionUI>();
        
        // Set monsterId from GameObject name if not set
        if (string.IsNullOrEmpty(monsterId))
        {
            monsterId = gameObject.name;
        }
        
        // Initialize all evolutions as locked
        InitializeDefaultLockedState();
    }

    private void InitializeDefaultLockedState()
    {
        // Set default visual state - all locked
        UpdateVisuals();
        UpdateEvolutionVisuals();
    }

    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        UpdateVisuals();
    }

    public void SetEvolutionLevel(int level)
    {
        maxEvolutionLevel = level;
    }

    public void SetMonsterCount(int count)
    {
        monsterCount = count;
    }

    // Set all unlocked evolutions
    public void SetUnlockedEvolutions(List<int> evolutions)
    {
        unlockedEvolutions = new List<int>(evolutions);
        // Set the evolution level to the highest unlocked evolution for display
        if (unlockedEvolutions.Count > 0)
        {
            maxEvolutionLevel = unlockedEvolutions.Max();
        }
        UpdateEvolutionVisuals();
    }

    public bool IsEvolutionUnlocked(int evolution)
    {
        return unlockedEvolutions.Contains(evolution);
    }

    public List<int> GetUnlockedEvolutions()
    {
        return new List<int>(unlockedEvolutions);
    }

    public string GetMonsterId()
    {
        return monsterId;
    }

    private void UpdateVisuals()
    {
        // Show monster as grayed out if not unlocked
        canvasGroup.DOFade(isUnlocked ? 1f : 1f, 0.2f).SetEase(Ease.Linear);
        canvasGroup.interactable = isUnlocked;
        canvasGroup.blocksRaycasts = isUnlocked;
    }

    private void UpdateEvolutionVisuals()
    {
        if (evolutionContainers == null || evolutionContainers.Length == 0) return;

        for (int i = 0; i < evolutionContainers.Length; i++)
        {
            if (evolutionContainers[i] == null) continue;

            int evolutionLevel = i + 1; // Convert 0-based index to 1-based evolution level
            bool isEvolutionUnlocked = isUnlocked && unlockedEvolutions.Contains(evolutionLevel);

            // Find locked and unlocked children
            Transform lockedChild = evolutionContainers[i].transform.Find("Locked");
            Transform unlockedChild = evolutionContainers[i].transform.Find("Unlocked");

            // Show locked state by default, unlocked only if both monster and evolution are unlocked
            if (lockedChild != null)
                lockedChild.gameObject.SetActive(!isEvolutionUnlocked);

            if (unlockedChild != null)
                unlockedChild.gameObject.SetActive(isEvolutionUnlocked);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Only show info if monster is unlocked
        if (isUnlocked)
        {
            monsterCollectionUI.OnShowInfo(GetComponent<RectTransform>().position.x, monsterCount, maxEvolutionLevel);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        monsterCollectionUI.OnHideInfo();
    }
}
