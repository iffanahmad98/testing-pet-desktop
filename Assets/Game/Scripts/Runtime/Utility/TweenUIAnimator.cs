using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class TweenUIAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public Sprite[] frames;
    public float frameRate = 30f;
    public bool loop = true;
    public bool playOnEnable = true;

    [Header("Smooth Transitions")]
    public bool useSmoothTransition = true;
    public Ease transitionEase = Ease.OutQuad;
    public float transitionDuration = 0.05f;

    [Header("DOTween Effects")]
    public bool useScaleEffect = false;
    public Vector3 scaleMultiplier = Vector3.one * 1.1f;
    public Ease scaleEase = Ease.OutElastic;

    public bool useRotationEffect = false;
    public float rotationAmount = 5f;
    public Ease rotationEase = Ease.InOutSine;

    public bool useShakeEffect = false;
    public float shakeStrength = 10f;
    public int shakeVibrato = 10;

    [Header("Fade In/Out")]
    public bool useFadeInOnStart = false;
    public float fadeInDuration = 0.3f;
    public Ease fadeInEase = Ease.OutQuad;

    public bool useFadeOutOnComplete = false;
    public float fadeOutDuration = 0.3f;
    public Ease fadeOutEase = Ease.OutQuad;

    public float TotalDuration => frames != null ? frames.Length / frameRate : 0f;

    private Image image;
    private CanvasGroup canvasGroup;
    private int currentFrame;
    private bool isPlaying;
    private Coroutine animationCoroutine;
    private Vector3 originalScale;
    private Quaternion originalRotation;
    private Sequence effectSequence;

    public System.Action onAnimationComplete;

    void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null && useSmoothTransition)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        originalScale = transform.localScale;
        originalRotation = transform.localRotation;
    }

    void OnEnable()
    {
        ResetAnimation();
        if (playOnEnable)
            Play();
    }

    void OnDisable()
    {
        Stop();
        effectSequence?.Kill();
        canvasGroup?.DOKill();
    }

    private void UpdateSprite()
    {
        if (frames.Length == 0) return;
        if (image != null)
        {
            if (useSmoothTransition && canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.DOFade(0, transitionDuration / 2).SetEase(transitionEase).OnComplete(() =>
                {
                    if (image != null && this != null)
                    {
                        image.sprite = frames[currentFrame];
                        canvasGroup.DOFade(1, transitionDuration / 2).SetEase(transitionEase);
                    }
                });
            }
            else
            {
                image.sprite = frames[currentFrame];
            }
        }
    }

    public void Play()
    {
        if (isPlaying) return;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (useFadeInOnStart && canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.DOFade(1, fadeInDuration).SetEase(fadeInEase).OnComplete(() =>
            {
                isPlaying = true;
                animationCoroutine = StartCoroutine(AnimationCoroutine());
                PlayEffects();
            });
        }
        else
        {
            isPlaying = true;
            animationCoroutine = StartCoroutine(AnimationCoroutine());
            PlayEffects();
        }
    }

    private void PlayEffects()
    {
        effectSequence?.Kill();
        effectSequence = DOTween.Sequence();

        if (useScaleEffect)
        {
            effectSequence.Join(transform.DOScale(Vector3.Scale(originalScale, scaleMultiplier), TotalDuration / 2).SetEase(scaleEase));
            effectSequence.Append(transform.DOScale(originalScale, TotalDuration / 2).SetEase(scaleEase));
        }

        if (useRotationEffect)
        {
            effectSequence.Join(transform.DORotate(new Vector3(0, 0, rotationAmount), TotalDuration / 2).SetEase(rotationEase).SetRelative(true));
            effectSequence.Append(transform.DORotate(new Vector3(0, 0, -rotationAmount), TotalDuration / 2).SetEase(rotationEase).SetRelative(true));
        }

        if (useShakeEffect)
        {
            effectSequence.Join(transform.DOShakePosition(TotalDuration, shakeStrength, shakeVibrato));
        }

        if (loop)
        {
            effectSequence.SetLoops(-1, LoopType.Restart);
        }

        effectSequence.Play();
    }

    private IEnumerator AnimationCoroutine()
    {
        currentFrame = 0;
        UpdateSprite();

        float frameTimeAccumulator = 0f;
        float frameDuration = 1f / frameRate;

        while (isPlaying)
        {
            frameTimeAccumulator += Time.deltaTime;

            if (frameTimeAccumulator >= frameDuration)
            {
                int framesToAdvance = Mathf.FloorToInt(frameTimeAccumulator / frameDuration);
                frameTimeAccumulator -= frameDuration * framesToAdvance;

                currentFrame += framesToAdvance;

                if (currentFrame >= frames.Length)
                {
                    if (loop)
                        currentFrame %= frames.Length;
                    else
                    {
                        currentFrame = frames.Length - 1;
                        isPlaying = false;
                        effectSequence?.Kill();
                        transform.localScale = originalScale;
                        transform.localRotation = originalRotation;

                        if (useFadeOutOnComplete && canvasGroup != null)
                        {
                            canvasGroup.DOFade(0, fadeOutDuration).SetEase(fadeOutEase).OnComplete(() =>
                            {
                                onAnimationComplete?.Invoke();
                            });
                        }
                        else
                        {
                            onAnimationComplete?.Invoke();
                        }
                        break;
                    }
                }

                UpdateSprite();
            }

            yield return null;
        }
    }

    public void Stop()
    {
        isPlaying = false;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        effectSequence?.Kill();
        transform.localScale = originalScale;
        transform.localRotation = originalRotation;

        currentFrame = 0;
        UpdateSprite();
    }

    public void Pause()
    {
        isPlaying = false;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        effectSequence?.Pause();
    }

    public void ResetAnimation()
    {
        currentFrame = 0;
        transform.localScale = originalScale;
        transform.localRotation = originalRotation;
        UpdateSprite();
    }

    public void PlayFromFrame(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < frames.Length)
        {
            currentFrame = frameIndex;
            UpdateSprite();
        }
        isPlaying = true;
        PlayEffects();
    }
}