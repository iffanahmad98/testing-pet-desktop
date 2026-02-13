using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleLifetimeByHeight : MonoBehaviour
{
    [Header("Height Reference")]
    [SerializeField] private float minHeight = 270f;
    [SerializeField] private float maxHeight = 1080f;   // default acuan
    [SerializeField] private float absoluteMaxHeight = 1200f;

    [Header("Lifetime Range")]
    [SerializeField] private float minLifetime = 2f;
    [SerializeField] private float maxLifetime = 4f;

    [Header ("Settings Reference")]
    [SerializeField] SettingsManager settingsManager;
    private ParticleSystem ps;
    private ParticleSystem.MainModule main;

    float lastHeight;
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        main = ps.main;

        // Default pakai 1080
        // ApplyLifetime(1080f);
    }

    void Update () {
       ApplyLifetime (settingsManager.GetSavedMaxGameAreaHeight ());
    }

    /// <summary>
    /// Set lifetime particle berdasarkan height
    /// </summary>
    public void ApplyLifetime(float curHeight)
    {
       // Debug.Log ("Cur Height" + curHeight);
        // Debug.Log ("Lifetime : " + curHeight);
        // Clamp height agar aman
        float clampedHeight = Mathf.Clamp(
            curHeight,
            minHeight,
            absoluteMaxHeight
        );

        // Normalisasi height (270 -> 1080)
        float t = Mathf.InverseLerp(
            minHeight,
            maxHeight,
            clampedHeight
        );

        // Hitung lifetime
        float lifetime = Mathf.Lerp(
            minLifetime,
            maxLifetime,
            t
        );

        // Safety clamp
        lifetime = Mathf.Clamp(lifetime, minLifetime, maxLifetime);

        main.startLifetime = lifetime;
    }
}
