using UnityEngine;
using DG.Tweening;

public class CloudShade : MonoBehaviour
{
    public int id = 0;
    [SerializeField] SpriteRenderer cloudRenderer;

    [Header("Destroy Time")]
    public int minDestroyTime = 4;
    public int maxDestroyTime = 8;

    [Header("Movement")]
    public int minSpeedTime = 5;
    public int maxSpeedTime = 10;
    private float speed;

    [Header("Rotation")]
    public Vector3 cloudRotation;

    // ===== EVENT TETAP =====
    public event System.Action<int> DestroyedEvent;

    private float lifeTime;
    private Sequence lifeSequence;

    // ===== FUNCTION EVENT TETAP =====
    public void AddDestroyedEvent(System.Action<int> destroyEvent)
    {
        DestroyedEvent += destroyEvent;
    }

    void Start()
    {
        transform.rotation = Quaternion.Euler(cloudRotation);

        // Alpha awal = 51 / 255
        Color c = cloudRenderer.color;
        c.a = 51f / 255f;
        cloudRenderer.color = c;

        // Lifetime di-random SEKALI
        lifeTime = Random.Range(minDestroyTime, maxDestroyTime);

        // Speed
        speed = Random.Range(minSpeedTime, maxSpeedTime) * 0.1f;

        StartLifecycle();
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    void StartLifecycle()
    {
        float halfTime = lifeTime * 0.5f;
        float fadeDuration = lifeTime - halfTime;

        lifeSequence = DOTween.Sequence();

        lifeSequence
            .AppendInterval(halfTime)
            .Append(
                cloudRenderer
                    .DOFade(0f, fadeDuration)
                    .SetEase(Ease.Linear)
            )
            .SetUpdate(true) // tetap jalan walau Time.timeScale = 0
            .OnComplete(SelfDestruct);
    }

    void SelfDestruct()
    {
        // EVENT TERPANGGIL SEBELUM DESTROY
        DestroyedEvent?.Invoke(id);

        Destroy(gameObject);
    }

    void OnDestroy()
    {
        lifeSequence?.Kill();
    }
}
