using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using Spine.Unity;

public class GachaResultPanelByEggs : MonoBehaviour
{
    [Header("Core Elements")]
    public GameObject root;
    public TextMeshProUGUI monsterNameText;
    public TextMeshProUGUI rarityText;
    private MonsterDataSO monsterData;

    [Header("Display Objects")]
    public GameObject monsterDisplay;
    public SkeletonGraphic monsterSkeletonGraphic;
    public Material monsterMaterial; 

    [Header("Effects")]
    public UIParticle shineVFX;
    public UIParticle confettiVFX;
    public UIParticle fireworkVFX;
    public UIParticle[] miniFireworkVFX;

    [Header("Buttons")]
    public Button spawnBtn;
    public Button sellBtn;
    public TextMeshProUGUI sellPriceText;

    [Header("Tween Eases")]
    public Ease fadeInRootEase = Ease.OutQuad;
    public Ease fadeInMonsterEase = Ease.OutQuad;
    public Ease punchMonsterEase = Ease.OutElastic;

    private CanvasGroup canvasGroup;
    private CanvasGroup monsterCanvas;

    private void Start()
    {
        // Ensure all necessary components are present
        if (root == null|| monsterDisplay == null ||
            monsterNameText == null || rarityText == null)
        {
            Debug.LogError("GachaResultPanel: Missing required UI elements!");
            return;
        }

        canvasGroup = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
        monsterCanvas = monsterDisplay.GetComponent<CanvasGroup>() ?? monsterDisplay.AddComponent<CanvasGroup>();
        //eggAnimatorUI = egg.GetComponent<TweenUIAnimator>() ?? egg.AddComponent<TweenUIAnimator>();


        // Reset all states
        ResetAllStates();

    }

    public void Show(MonsterDataSO monster, System.Action onSell, System.Action onSpawn)
    {
        ResetAllStates();
        monsterData = monster;
        
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1.5f);
        // 1. Fade in root + scale punch
        seq.AppendCallback(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            monsterCanvas.interactable = true;
            monsterCanvas.blocksRaycasts = true;
        });
        seq.Append(canvasGroup.DOFade(1, 0.2f).SetEase(fadeInRootEase));
        
        // 4. Show monster info 
        seq.AppendCallback(() =>
        {
            monsterNameText.text = monster.name;
            monsterSkeletonGraphic.skeletonDataAsset = monster.monsterSpine[0];
            monsterSkeletonGraphic.material = monsterMaterial;
            monsterSkeletonGraphic.startingAnimation = monsterSkeletonGraphic.skeletonDataAsset.GetSkeletonData(true).FindAnimation("idle")?.Name ?? "idle";
            monsterSkeletonGraphic.Initialize(true);
            monsterSkeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
            rarityText.text = monster.monType.ToString().ToUpperInvariant();
            sellPriceText.text = monster.sellPriceStage1.ToString();
        });
        // 5. Monster display: fade in + scale punch
        seq.Append(monsterCanvas.DOFade(1, 0.2f).SetEase(fadeInMonsterEase));
        seq.Join(monsterDisplay.transform.DOPunchScale(Vector3.one * 1.25f, 0.4f, 8, 0.8f).SetEase(punchMonsterEase));
        seq.JoinCallback(() => confettiVFX?.Play());
        seq.JoinCallback(() =>
        {
            fireworkVFX.gameObject.SetActive(true);
            fireworkVFX?.Play();
        });
        seq.AppendCallback(() => StartCoroutine(PlayMiniFireworksWithDelay()));
        seq.AppendCallback(() =>
        {
            shineVFX.gameObject.SetActive(false);
            sellBtn.onClick.RemoveAllListeners();
            spawnBtn.onClick.RemoveAllListeners();
            sellBtn.onClick.AddListener(() =>
            {
                onSell?.Invoke();
                HideResultPanel();
            });
            spawnBtn.onClick.AddListener(() =>
            {
                onSpawn?.Invoke();
                HideResultPanel();
            });
            
        });
        seq.Play();
    }

    private void ResetAllStates()
    {
        // Ensure root is active and reset scale
        root.SetActive(true);
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;


        // Hide and reset monster display
        if (monsterCanvas != null) monsterCanvas.alpha = 0f;
        monsterCanvas.interactable = false;
        monsterCanvas.blocksRaycasts = false;

        // Stop effects
        shineVFX.Stop();
        confettiVFX.Stop();
        fireworkVFX?.Stop();
        foreach (var miniFirework in miniFireworkVFX)
        {
            if (miniFirework != null) miniFirework.Stop();
        }
    }

    private void HideResultPanel()
    {
        var canvasGroup = root.GetComponent<CanvasGroup>();
        
        if (canvasGroup != null)
        {
            canvasGroup.DOFade(0, 0.5f).OnComplete(() =>
            {
                root.SetActive(false);
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            });

            monsterCanvas.DOFade(0, 0.5f).OnComplete(() =>
            {
                monsterCanvas.alpha = 0f;
                monsterCanvas.interactable = false;
                monsterCanvas.blocksRaycasts = false;
            });
        }
        else
        {
            root.SetActive(false);
        }
    }

    private IEnumerator PlayMiniFireworksWithDelay()
    {
        foreach (var miniFirework in miniFireworkVFX)
        {
            miniFirework.gameObject.SetActive(true);
            miniFirework?.Play();
            yield return new WaitForSeconds(0.5f);
        }
    }

}
