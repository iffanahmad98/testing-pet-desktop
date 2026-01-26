using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PumpkinCarUIAnimator : MonoBehaviour
{
    [Header("Target UI Image")]
    [SerializeField] private Image img;

    [Header("Frames")]
    [SerializeField] private Sprite[] frames;

    [Header("Speed")]
    [SerializeField] private float fps = 12f;

    private int frameIndex = 0;
    private Coroutine animCo;

    private void Awake()
    {
        if (!img) img = GetComponent<Image>();
        SetFrame(0); // default to first frame
    }

    private void SetFrame(int i)
    {
        if (frames == null || frames.Length == 0 || img == null) return;
        img.sprite = frames[Mathf.Clamp(i, 0, frames.Length - 1)];
    }

    public void Play()
    {
        if (frames == null || frames.Length == 0) return;
        if (animCo != null) return;          // biar gak dobel coroutine
        Debug.Log("Pumpkin animator Play()");
        animCo = StartCoroutine(Animate());
    }

    public void Pause()
    {
        if (animCo == null) return;
        Debug.Log("Pumpkin animator Pause()");
        StopCoroutine(animCo);
        animCo = null;
    }

    public void ResetAnim()
    {
        Pause();
        frameIndex = 0;
        SetFrame(0);
        Debug.Log("Pumpkin animator ResetAnim()");
    }

    private IEnumerator Animate()
    {
        float interval = 1f / Mathf.Max(1f, fps);
        Debug.Log("Pumpkin animator interval = " + interval);
        var wait = new WaitForSeconds(interval);

        while (true)
        {
            SetFrame(frameIndex);
            frameIndex = (frameIndex + 1) % frames.Length; // loop balik ke 00
            yield return wait;
        }
    }

}
