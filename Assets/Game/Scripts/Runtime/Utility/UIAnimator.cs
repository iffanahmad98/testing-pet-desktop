using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIAnimator : MonoBehaviour
{
    public Sprite[] frames;             // Your sprite sheet split into frames
    public float frameRate = 30f;       // Frames per second
    public bool loop = true;
    public bool playOnEnable = true;    // Whether to auto-play when enabled
    
    /// <summary>
    /// Get the total duration of the animation in seconds
    /// </summary>
    public float TotalDuration => frames != null ? frames.Length / frameRate : 0f;

    private Image image;
    private int currentFrame;
    // private float timer;
    private bool isPlaying;
    private Coroutine animationCoroutine;  // Add this field to track the coroutine
    
    // Event that fires when animation completes (only in non-loop mode)
    public System.Action onAnimationComplete;

    void Awake()
    {
        image = GetComponent<Image>();
    }

    void OnEnable()
    {
        ResetAnimation();
        if (playOnEnable)
            Play();
    }
    
    private void UpdateSprite()
    {
        if (frames.Length == 0) return;
        if (image != null)
            image.sprite = frames[currentFrame];
    }
    
    // Public API methods
    
    /// <summary>
    /// Start playing the animation
    /// </summary>
    public void Play()
    {
        if (isPlaying) return;
        
        // Stop any existing animation coroutine first
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        isPlaying = true;
        animationCoroutine = GameManager.instance.StartCoroutine(AnimationCoroutine());
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
            
            // Advance frame when enough time has passed
            if (frameTimeAccumulator >= frameDuration)
            {
                // Handle potential frame skips in case of performance hiccups
                int framesToAdvance = Mathf.FloorToInt(frameTimeAccumulator / frameDuration);
                frameTimeAccumulator -= frameDuration * framesToAdvance;
                
                // Advance by calculated number of frames
                currentFrame += framesToAdvance;
                
                if (currentFrame >= frames.Length)
                {
                    if (loop)
                        currentFrame %= frames.Length; // Wrap around properly
                    else
                    {
                        currentFrame = frames.Length - 1;
                        isPlaying = false;
                        onAnimationComplete?.Invoke();
                        break;
                    }
                }
                
                UpdateSprite();
            }
            
            yield return null; // Wait for next frame
        }
    }
    
    /// <summary>
    /// Stop the animation and reset to first frame
    /// </summary>
    public void Stop()
    {
        isPlaying = false;
        
        // Stop the animation coroutine
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
        
        currentFrame = 0;
        UpdateSprite();
    }
    
    /// <summary>
    /// Pause the animation at current frame
    /// </summary>
    public void Pause()
    {
        isPlaying = false;
        
        // Stop the animation coroutine
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }
    
    /// <summary>
    /// Reset animation to first frame without changing play state
    /// </summary>
    public void ResetAnimation()
    {
        currentFrame = 0;
        // timer = 0f;
        UpdateSprite();
    }
    
    /// <summary>
    /// Play animation starting from a specific frame
    /// </summary>
    public void PlayFromFrame(int frameIndex)
    {
        if (frameIndex >= 0 && frameIndex < frames.Length)
        {
            currentFrame = frameIndex;
            UpdateSprite();
        }
        isPlaying = true;
    }
}
