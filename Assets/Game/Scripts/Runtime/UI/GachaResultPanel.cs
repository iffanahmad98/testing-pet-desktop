using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Coffee.UIExtensions;
using DG.Tweening;
using System.Collections;

public class GachaResultPanel : MonoBehaviour
{
    [Header("Core Elements")]
    public GameObject root;
    public Image monsterImage;
    public TextMeshProUGUI monsterNameText;
    public TextMeshProUGUI rarityText;

    [Header("Display Objects")]
    public GameObject chest;
    public GameObject egg;
    public GameObject monsterDisplay;

    [Header("Effects")]
    public UIParticle shineVFX;
    public UIParticle confettiVFX;

    [Header("Buttons")]
    public Button backButton;
    public Button rollAgainButton;

    [Header("Tween Eases")]
    public Ease fadeInRootEase = Ease.OutQuad;
    public Ease fadeInChestEase = Ease.OutQuad;
    public Ease fadeOutChestEase = Ease.InQuad;
    public Ease fadeInEggEase = Ease.OutQuad;
    public Ease punchEggEase = Ease.OutElastic;
    public Ease fadeOutEggEase = Ease.InQuad;
    public Ease fadeInMonsterEase = Ease.OutQuad;
    public Ease punchMonsterEase = Ease.OutElastic;

    private System.Action onFinishGacha;

    private CanvasGroup canvasGroup;
    private CanvasGroup chestCanvas;
    private CanvasGroup eggCanvas;
    private CanvasGroup monsterCanvas;
    private Animator chestAnimator;
    private Animator eggAnimator;

    private void Start()
    {
        // Ensure all necessary components are present
        if (root == null || chest == null || egg == null || monsterDisplay == null ||
            monsterNameText == null || rarityText == null || backButton == null || rollAgainButton == null)
        {
            Debug.LogError("GachaResultPanel: Missing required UI elements!");
            return;
        }

        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() => HideResultPanel());

        canvasGroup = root.GetComponent<CanvasGroup>() ?? root.AddComponent<CanvasGroup>();
        chestCanvas = chest.GetComponent<CanvasGroup>() ?? chest.AddComponent<CanvasGroup>();
        eggCanvas = egg.GetComponent<CanvasGroup>() ?? egg.AddComponent<CanvasGroup>();
        monsterCanvas = monsterDisplay.GetComponent<CanvasGroup>() ?? monsterDisplay.AddComponent<CanvasGroup>();
        chestAnimator = chest.GetComponent<Animator>() ?? chest.AddComponent<Animator>();
        eggAnimator = egg.GetComponent<Animator>() ?? egg.AddComponent<Animator>();

        // Reset all states
        ResetAllStates();

    }

    public void Show(MonsterDataSO monster, System.Action onRollAgain, System.Action onComplete)
    {
        ResetAllStates();
        onFinishGacha = onComplete;

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
            chestAnimator.Rebind();
            chestAnimator.Update(0f);
            chestAnimator.Play("chest", 0, 0f);
        });
        seq.AppendInterval(GetClipLength(chestAnimator, "chest"));
        seq.Append(chestCanvas.DOFade(0, 0.2f).SetEase(fadeOutChestEase));
        // 3. Egg: set active, fade in + scale punch, play animator, on last frame play shineVFX
        seq.Append(eggCanvas.DOFade(1, 0.2f).SetEase(fadeInEggEase));
        seq.Join(egg.transform.DOPunchScale(Vector3.one * 1.25f, 0.4f, 8, 0.8f).SetEase(punchEggEase));
        seq.JoinCallback(() =>
        {
            egg.SetActive(true);
            eggAnimator.Rebind();
            eggAnimator.Update(0f);
            eggAnimator.Play("egg", 0, 0f);
        });
        seq.AppendInterval(GetClipLength(eggAnimator, "egg"));
        seq.JoinCallback(() => shineVFX?.Play());
        seq.Append(eggCanvas.DOFade(0, 0.2f).SetEase(fadeOutEggEase));
        // 4. Show monster info 
        seq.AppendCallback(() =>
        {
            monsterNameText.text = monster.monsterName;
            rarityText.text = monster.monType.ToString().ToUpperInvariant();
        });
        // 5. Monster display: fade in + scale punch
        seq.Append(monsterCanvas.DOFade(1, 0.2f).SetEase(fadeInMonsterEase));
        seq.Join(monsterDisplay.transform.DOPunchScale(Vector3.one * 1.25f, 0.4f, 8, 0.8f).SetEase(punchMonsterEase));
        seq.JoinCallback(() => confettiVFX?.Play());
        seq.AppendCallback(() =>
        {
            rollAgainButton.onClick.RemoveAllListeners();
            rollAgainButton.onClick.AddListener(() => onRollAgain?.Invoke());
            onFinishGacha?.Invoke();
        });
        seq.Play();
    }

    private void ResetAllStates()
    {
        // Ensure root is active and reset scale
        root.SetActive(true);
        var canvasGroup = root.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Hide and reset chest
        chest.SetActive(false);
        chestAnimator.ResetTrigger("Open");
        var chestCanvas = chest.GetComponent<CanvasGroup>();
        if (chestCanvas != null) chestCanvas.alpha = 0f;

        // Hide and reset egg
        eggAnimator.ResetTrigger("Crack");
        var eggCanvas = egg.GetComponent<CanvasGroup>();
        if (eggCanvas != null) eggCanvas.alpha = 0f;

        // Hide and reset monster display
        var monsterCanvas = monsterDisplay.GetComponent<CanvasGroup>();
        if (monsterCanvas != null) monsterCanvas.alpha = 0f;
        monsterCanvas.interactable = false;
        monsterCanvas.blocksRaycasts = false;

        // Stop effects
        shineVFX?.Stop();
        confettiVFX?.Stop();
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
                monsterDisplay.SetActive(false);
                monsterCanvas.alpha = 0f;
                monsterCanvas.interactable = false;
                monsterCanvas.blocksRaycasts = false;
            });
        }
        else
        {
            root.SetActive(false);
            onFinishGacha?.Invoke();
        }
    }

    private IEnumerator WaitForAnimation(Animator animator, string stateName)
    {
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;
        while (animator.IsInTransition(0) || animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
            yield return null;
    }
    
    float GetClipLength(Animator anim, string clipName)
    {
        // quickest clip-lookup; cache this if you call it often
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
            if (clip.name == clipName) return clip.length;
        return 0f;
    }
}
