using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
  EerieFlicker.cs
  ----------------------------------------------------
  goal: make a hospital hallway light that "has seen things"
  vibe: not a club strobe, not a nightlight—somewhere in the
        unsettling middle with occasional panic attacks.
*/

public class EerieFlicker : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Light we actually mess with. If empty, I’ll auto-grab the Light on this object.")]
    public Light targetLight;

    [Tooltip("Optional: bulb/fixture renderers if you want the mesh emission to blink too.")]
    public List<Renderer> emissiveRenderers = new List<Renderer>();

    [Header("Brightness")]
    [Tooltip("Normal brightness when the light is behaving (rare).")]
    public float baseIntensity = 1.6f;

    [Tooltip("Tiny hand-tremor on intensity so it never sits perfectly still.")]
    public float jitterIntensity = 0.35f;

    [Tooltip("How strong the bulb glow looks. If your bulb feels shy, turn this up.")]
    public float emissionBoost = 2.0f;

    [Tooltip("Color of the fake 'glow'. Warm = cozy apocalypse, cool = hospital bills.")]
    public Color emissionColor = Color.white;

    [Header("Timing (the spook sauce)")]
    [Tooltip("How long it stays 'on' each cycle (random between these). Not too fast, not too slow.")]
    public Vector2 onRange = new Vector2(0.12f, 0.42f);

    [Tooltip("Short blackouts. Like the light forgot its password.")]
    public Vector2 offRange = new Vector2(0.06f, 0.22f);

    [Tooltip("Sometimes it really dies for a bit, for dramatic effect.")]
    public float longOffChance = 0.08f;
    public Vector2 longOffRange = new Vector2(0.5f, 1.25f);

    [Tooltip("Occasional double-blink to keep players guessing.")]
    public float hardBlinkChance = 0.22f;

    [Header("Noise seasoning")]
    [Tooltip("Perlin noise speed. Low = lazy shimmer. High = nervous energy.")]
    public float noiseSpeed = 5.5f;

    [Tooltip("How much the Perlin noise actually moves intensity.")]
    public float noiseAmount = 0.12f;

    [Header("Audio (optional but nice)")]
    [Tooltip("Looping low buzz for when it’s on. Adds 30% extra sadness.")]
    public AudioSource buzzLoop;

    [Tooltip("Short zap for hard blinks. Tasteful chaos.")]
    public AudioSource zapOneShot;

    // internals (pls don’t judge)
    MaterialPropertyBlock _mpb;
    static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");
    float _seed;

    void Reset()
    {
        // be helpful: if this script sits on a Light, just use it
        if (!targetLight) targetLight = GetComponent<Light>();
        if (!targetLight) targetLight = GetComponentInChildren<Light>();
    }

    void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        _seed = Random.value * 1000f;

        if (targetLight != null)
            targetLight.intensity = baseIntensity;

        // make sure emission is allowed to glow
        for (int i = 0; i < emissiveRenderers.Count; i++)
        {
            var mats = emissiveRenderers[i].sharedMaterials;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m] != null) mats[m].EnableKeyword("_EMISSION");
            }
        }
    }

    void OnEnable()
    {
        StartCoroutine(FlickerLoop());
    }

    IEnumerator FlickerLoop()
    {
        while (true)
        {
            // --- ON PHASE: looks alive but clearly needs therapy ---
            float onTime = Random.Range(onRange.x, onRange.y);
            float t = 0f;

            SetLightActive(true);

            while (t < onTime)
            {
                t += Time.deltaTime;

                // base + small hand jitter + perlin shimmer
                float handJitter = Random.Range(-jitterIntensity, jitterIntensity) * 0.5f;
                float n = Mathf.PerlinNoise(_seed, Time.time * noiseSpeed) * 2f - 1f;
                float noisy = baseIntensity + handJitter + (n * noiseAmount);

                ApplyLight(noisy);
                ApplyEmission(noisy);

                yield return null;
            }

            // occasionally do a dramatic double blink because horror pacing™
            if (Random.value < hardBlinkChance)
                yield return HardBlink();

            // --- OFF PHASE: short nap or full system restart ---
            float offTime = (Random.value < longOffChance)
                ? Random.Range(longOffRange.x, longOffRange.y)
                : Random.Range(offRange.x, offRange.y);

            SetLightActive(false);
            yield return new WaitForSeconds(offTime);
        }
    }

    IEnumerator HardBlink()
    {
        // quick off
        SetLightActive(false);
        if (zapOneShot) zapOneShot.Play();
        yield return new WaitForSeconds(Random.Range(0.06f, 0.16f));

        // dramatic spike back on
        float spike = baseIntensity + jitterIntensity * 1.4f;
        ApplyLight(spike);
        ApplyEmission(spike * 1.2f);
        SetLightActive(true);
        yield return new WaitForSeconds(Random.Range(0.08f, 0.18f));

        // and… down again
        SetLightActive(false);
        yield return new WaitForSeconds(Random.Range(0.05f, 0.12f));
    }

    void SetLightActive(bool on)
    {
        if (targetLight)
        {
            targetLight.enabled = on;
            if (buzzLoop)
            {
                if (on && !buzzLoop.isPlaying) buzzLoop.Play();
                if (!on && buzzLoop.isPlaying) buzzLoop.Pause();
            }
        }

        // mesh emission tracks the vibe too
        float val = on ? baseIntensity : 0f;
        ApplyEmission(val);
    }

    void ApplyLight(float intensity)
    {
        if (!targetLight) return;
        targetLight.intensity = Mathf.Max(0f, intensity);
    }

    void ApplyEmission(float likeIntensity)
    {
        if (emissiveRenderers == null || emissiveRenderers.Count == 0) return;

        // pretend the bulb glow mirrors the light, then cheat it brighter
        Color e = emissionColor * Mathf.LinearToGammaSpace(Mathf.Max(0f, likeIntensity) * emissionBoost);

        for (int i = 0; i < emissiveRenderers.Count; i++)
        {
            var r = emissiveRenderers[i];
            if (!r) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(_EmissionColor, e);
            r.SetPropertyBlock(_mpb);
        }
    }
}
