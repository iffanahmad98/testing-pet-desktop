using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;
using Spine.Unity;

public class GachaResultPanel : MonoBehaviour
{
    [Header("Core Elements")]
    public GameObject root;
    public TextMeshProUGUI monsterNameText;
    public TextMeshProUGUI rarityText;
    private MonsterDataSO monsterData;

    [Header("Display Objects")]
    public GameObject chest;
    public GameObject egg;
    public SkeletonGraphic eggMonsterGraphic;
    public CanvasGroup eggMonsterCanvasGroup;
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
    public Ease fadeInChestEase = Ease.OutQuad;
    public Ease fadeOutChestEase = Ease.InQuad;
    public Ease fadeInEggEase = Ease.OutQuad;
    public Ease punchEggEase = Ease.OutElastic;
    public Ease fadeOutEggEase = Ease.InQuad;
    public Ease fadeInMonsterEase = Ease.OutQuad;
    public Ease punchMonsterEase = Ease.OutElastic;

    private CanvasGroup canvasGroup;
    private CanvasGroup chestCanvas;
    private CanvasGroup eggCanvas;
    private CanvasGroup monsterCanvas;
    
    private UIAnimator chestAnimatorUI;
    private TweenUIAnimator eggAnimatorUI;
    private Animator eggAnimator; // TEST
    public Coroutine coroutineFirework;
    private void Start()
    {
        // Ensure all necessary components are present
        if (root == null || chest == null || egg == null || monsterDisplay == null ||
            monsterNameText == null || rarityText == null)
        {
            Debug.LogError("GachaResultPanel: Missing required UI elements!");
            return;
        }

        canvasGroup = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
        chestCanvas = chest.GetComponent<CanvasGroup>() ?? chest.AddComponent<CanvasGroup>();
        eggCanvas = egg.GetComponent<CanvasGroup>() ?? egg.AddComponent<CanvasGroup>();
        monsterCanvas = monsterDisplay.GetComponent<CanvasGroup>() ?? monsterDisplay.AddComponent<CanvasGroup>();
        chestAnimatorUI = chest.GetComponent<UIAnimator>() ?? chest.AddComponent<UIAnimator>();
        //eggAnimatorUI = egg.GetComponent<TweenUIAnimator>() ?? egg.AddComponent<TweenUIAnimator>();
        eggAnimator = egg.GetComponent<Animator>() ?? egg.AddComponent<Animator>();


        // Reset all states
        ResetAllStates();

    }

    public void Show(MonsterDataSO monster, System.Action onSell, System.Action onSpawn)
    {
        ResetAllStates();
        monsterData = monster;

        Sequence seq = DOTween.Sequence();

        // 1. Fade in root + scale punch
        seq.AppendCallback(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            monsterCanvas.interactable = true;
            monsterCanvas.blocksRaycasts = true;
        });
        seq.Append(canvasGroup.DOFade(1, 0.2f).SetEase(fadeInRootEase));
        // 2. Chest: set active, fade in, play animator, fade out
        seq.Append(chestCanvas.DOFade(1, 0.2f).SetEase(fadeInChestEase));
        seq.JoinCallback(() =>
        {
            chest.SetActive(true);
            chestAnimatorUI?.Play();
        });
        seq.AppendInterval(chestAnimatorUI?.TotalDuration * 2f ?? 1f);
        seq.Append(chestCanvas.DOFade(0, 0.2f).SetEase(fadeOutChestEase));
        // 3. Egg: set active, fade in + scale punch, play animator, on last frame play shineVFX
        seq.Append(eggCanvas.DOFade(1, 0.2f).SetEase(fadeInEggEase));
        seq.Join(egg.transform.DOPunchScale(Vector3.one * 1.25f, 0.4f, 8, 0.8f).SetEase(punchEggEase));
        seq.JoinCallback(() =>
        {
            egg.SetActive(true);
            eggAnimatorUI?.Play();
            eggAnimator?.SetTrigger("Crack");
        });
        
        // Wait for egg animation to complete, then fade in egg monster
        seq.AppendInterval(1.5f);
        seq.AppendCallback(() =>
        {
            // Fade in egg monster and assign spine graphic
            eggMonsterGraphic.skeletonDataAsset = monster.monsterSpine[0];
            eggMonsterGraphic.material = monsterMaterial;
            eggMonsterGraphic.startingAnimation = eggMonsterGraphic.skeletonDataAsset.GetSkeletonData(true).FindAnimation("idle")?.Name ?? "idle";
            eggMonsterGraphic.Initialize(true);
            eggMonsterCanvasGroup.DOFade(1, 0.5f);
            shineVFX?.gameObject.SetActive(true);
            shineVFX?.Play();
        });
        seq.AppendInterval(1f);
        seq.Append(eggCanvas.DOFade(0, 0.2f).SetEase(fadeOutEggEase));
        // 4. Show monster info 
        seq.AppendCallback(() =>
        {
           // monsterNameText.text = monster.name;
            monsterNameText.text = monster.monsterName;
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
       // if (coroutineFirework == null) {
       seq.AppendCallback(() =>
{
    coroutineFirework = GameManager.instance.StartCoroutine(PlayMiniFireworksWithDelay());
});
      //  }
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

        // Hide and reset chest
        chest.SetActive(false);
        if (chestCanvas != null) chestCanvas.alpha = 0f;

        // Hide and reset egg
        egg.SetActive(false);
        if (eggCanvas != null) eggCanvas.alpha = 0f;

        // Reset egg monster
        if (eggMonsterCanvasGroup != null) eggMonsterCanvasGroup.alpha = 0f;

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
        Debug.Log ("Null");
        coroutineFirework = null;
    }
}
