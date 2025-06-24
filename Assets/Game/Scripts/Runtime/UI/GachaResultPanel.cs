using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;

public class GachaResultPanel : MonoBehaviour
{
    [Header("Core Elements")]
    public GameObject root;
    public Image monsterImage;
    public TextMeshProUGUI monsterNameText;
    public TextMeshProUGUI rarityText;

    [Header("Sequence Objects")]
    public Sprite[] chestFrames; // Chest opening animation
    public Sprite[] eggFrames;   // Egg hatching animation

    [Header("Display Objects")]
    public Image chestImage;
    public Image eggImage;
    public GameObject monsterDisplay;

    [Header("Effects")]
    public GameObject confettiFxRender;
    public ParticleSystem confettiFx;
    public GameObject shineFxRender;
    public ParticleSystem shineFx;

    [Header("Buttons")]
    public Button backButton;
    public Button rollAgainButton;
    private Tween sequenceTween;
    [SerializeField] private float intervalBetweenFrames = 0.25f;
    [SerializeField] private float intervalBetweenAnimations = 0.8f;

    public void Show(MonsterDataSO monster, System.Action onRollAgain)
    {
        root.SetActive(true);
        ResetAllStates();

        // Begin the sequence
        sequenceTween = DOTween.Sequence()
       .AppendCallback(() => StartCoroutine(PlayFrameSequence(chestImage, chestFrames, intervalBetweenFrames))) // Chest
       .AppendInterval(chestFrames.Length * intervalBetweenFrames)
       .AppendCallback(() =>
       {
           chestImage.gameObject.SetActive(false);
           shineFx.Play();
           shineFxRender.SetActive(true);
           StartCoroutine(PlayFrameSequence(eggImage, eggFrames, intervalBetweenFrames)); // Egg
       })
       .AppendInterval(eggFrames.Length * intervalBetweenFrames)
       .AppendInterval(intervalBetweenAnimations)
       .AppendCallback(() =>
       {
           eggImage.gameObject.SetActive(false);
           monsterDisplay.SetActive(true);
           //    monsterImage.sprite = monster.monsIconImg[0];
           monsterNameText.text = monster.monsterName;
           //    rarityText.text = monster.monType.ToString().ToUpper();
       })
       .AppendCallback(() =>
       {
           confettiFxRender.SetActive(true);
           confettiFx.Play();
           shineFx.Stop();
           shineFxRender.SetActive(false);
       });


        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(() => Hide());

        rollAgainButton.onClick.RemoveAllListeners();
        rollAgainButton.onClick.AddListener(() =>
        {
            Hide();
            onRollAgain?.Invoke();
        });
    }

    public void Hide()
    {
        if (sequenceTween != null && sequenceTween.IsActive())
            sequenceTween.Kill();

        root.SetActive(false);
    }

    private void ResetAllStates()
    {
        chestImage.gameObject.SetActive(false);
        eggImage.gameObject.SetActive(false);
        confettiFxRender.SetActive(false);
        shineFxRender.SetActive(false);
        monsterDisplay.SetActive(false);
    }
    private IEnumerator PlayFrameSequence(Image targetImage, Sprite[] frames, float frameDelay)
    {
        targetImage.gameObject.SetActive(true);

        foreach (var frame in frames)
        {
            targetImage.sprite = frame;
            yield return new WaitForSeconds(frameDelay);
        }
    }
}
