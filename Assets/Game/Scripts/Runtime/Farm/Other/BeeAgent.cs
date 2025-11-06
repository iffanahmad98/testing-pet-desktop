using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BeeAgent : MonoBehaviour
{
    SpriteRenderer sr;
    Vector3 anchor;
    float lifeTime, speed, amp, freq;
    float seed;
    Action<BeeAgent> onDespawn;
    float alpha; // 0..1

    // Randomized fade durations untuk natural variation
    float fadeInDuration;
    float fadeOutDuration;

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

        // Randomize fade durations setiap kali spawn untuk variasi natural
        fadeInDuration = UnityEngine.Random.Range(0.2f, 0.6f);   // durasi fade in bervariasi
        fadeOutDuration = UnityEngine.Random.Range(0.3f, 0.7f);  // durasi fade out bervariasi
    }

    public void Spawn(Vector3 position, float life, float moveSpeed, float buzzAmp, float buzzFreq,
                      Action<BeeAgent> onDespawnCb)
    {
        transform.position = anchor = position;
        lifeTime = life;
        speed = moveSpeed;
        amp = buzzAmp;
        freq = buzzFreq;
        onDespawn = onDespawnCb;

        StopAllCoroutines();
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        // Fade in dengan durasi random
        yield return Fade(0f, 1f, fadeInDuration);

        float elapsed = 0f;
        Vector3 lastPos = transform.position;

        while (elapsed < lifeTime)
        {
            elapsed += Time.deltaTime;

            // Buzz pattern (lebih erratic dari butterfly, khas lebah)
            float t = Time.time;
            Vector2 noise = new Vector2(
                Mathf.PerlinNoise(seed, t * freq) - 0.5f,
                Mathf.PerlinNoise(seed + 199.3f, t * (freq * 1.1f)) - 0.5f
            );

            Vector2 drift = new Vector2(
                Mathf.Sin((t + seed) * 0.8f),
                Mathf.Cos((t + seed) * 0.9f)
            );

            Vector2 dir = (noise * amp * 2.2f + drift * 0.3f);
            Vector3 target = anchor + new Vector3(dir.x, dir.y, 0f);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * (2.5f + speed * 2.2f));

            Vector3 vel = transform.position - lastPos;

            // Rotate berdasarkan arah gerak (lebah lebih stabil rotasinya)
            float tilt = Mathf.Clamp(vel.x * 200f, -10f, 10f);
            transform.rotation = Quaternion.Euler(0f, 0f, tilt);

            // Bob scale (buzz effect - lebih cepat dari butterfly)
            float bob = 0.1f + Mathf.Sin((Time.time + seed) * (freq * 3f)) * 0.008f; // buzz lebih cepat
            transform.localScale = new Vector3(bob, bob, 0.1f);

            lastPos = transform.position;
            yield return null;
        }

        // Fade out dengan durasi random
        yield return Fade(1f, 0f, fadeOutDuration);
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
