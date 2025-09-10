using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ButterflyAgent : MonoBehaviour
{
    SpriteRenderer sr;
    Vector3 anchor;
    float lifeTime, speed, amp, freq;
    float seed;
    Action<ButterflyAgent> onDespawn;
    float alpha; // 0..1

    Animator anim;
    public string[] nameAnim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        seed = UnityEngine.Random.value * 1000f;
        alpha = 0f;
        SetAlpha(0f);
    }

    void OnEnable()
    {
        if (anim != null && nameAnim != null && nameAnim.Length > 0)
        {
            int idx = UnityEngine.Random.Range(0, nameAnim.Length);
            string chosenAnim = nameAnim[idx];
            anim.Play(chosenAnim, 0, UnityEngine.Random.value);
        }
    }

    public void Spawn(Vector3 position, float life, float moveSpeed, float flutterAmp, float flutterFreq,
                      Action<ButterflyAgent> onDespawnCb)
    {
        transform.position = anchor = position;
        lifeTime = life;
        speed = moveSpeed;
        amp = flutterAmp;
        freq = flutterFreq;
        onDespawn = onDespawnCb;

        StopAllCoroutines();
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        // Fade in
        yield return Fade(0f, 1f, 0.35f);

        float elapsed = 0f;
        Vector3 lastPos = transform.position;

        while (elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;

            // Wander/Flutter di sekitar anchor (gabung drift + noise)
            float t = Time.time;
            Vector2 noise = new Vector2(
                Mathf.PerlinNoise(seed, t * freq) - 0.5f,
                Mathf.PerlinNoise(seed + 133.7f, t * (freq * 0.9f)) - 0.5f
            );

            Vector2 drift = new Vector2(
                Mathf.Sin((t + seed) * 0.6f),
                Mathf.Cos((t + seed) * 0.7f)
            );

            Vector2 dir = (noise * amp * 2f + drift * 0.25f);
            Vector3 target = anchor + new Vector3(dir.x, dir.y, 0f);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * (2f + speed * 2f));

            Vector3 vel = transform.position - lastPos;

            // ROTATE tipis berdasarkan arah gerak (maks 12°)
            float tilt = Mathf.Clamp(vel.x * 250f, -12f, 12f);
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);

            // BOB scale (kesan naik–turun)
            float bob = 0.1f + Mathf.Sin((Time.time + seed) * (freq * 2f)) * 0.005f; // ±5%
            transform.localScale = new Vector3(bob, bob, 0.1f);

            lastPos = transform.position;
            yield return null;
        }

        // Fade out lalu despawn
        yield return Fade(1f, 0f, 0.4f);
        onDespawn?.Invoke(this);
    }

    IEnumerator Fade(float from, float to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            alpha = Mathf.Lerp(from, to, t / dur);
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        var c = sr.color;
        c.a = a;
        sr.color = c;
    }
}