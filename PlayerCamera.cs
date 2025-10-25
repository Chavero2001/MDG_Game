using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerCamera : MonoBehaviour
{
    [Header("Follow target")]
    public Transform cameraPosition;

    [Header("Volume (optional)")]
    public Volume volume;                        // Assign one, or we'll create a runtime global Volume.

    [Header("Vignette control")]
    [Range(0f, 1f)] public float baseIntensity = 0.6f;  // Baseline intensity
    public bool useLifePoints = true;                   // Subtract life influence like your original
    public float lifeScale = 20f;                       // intensity -= lifePoints / lifeScale

    [Header("Screen Shake")]
    [Tooltip("Max horizontal/forward shake offset in world units when amplitude=1.")]
    public Vector2 maxShakeXZ = new Vector2(0.6f, 0.6f);
    [Tooltip("Apply a small vertical nudge as well.")]
    public bool includeY = false;
    [Tooltip("Max vertical shake offset in world units when amplitude=1.")]
    public float maxShakeY = 0.15f;

    private Vignette vignette;
    private bool createdRuntimeVolume = false;          // Track if we created one to clean up later

    // --- Shake internals ---
    private float shakeTimer = 0f;
    private float shakeDuration = 0f;
    private float shakeAmplitude = 0f;
    private float shakeFrequency = 18f;
    private Vector3 shakeOffset = Vector3.zero;
    private Vector3 shakeSeeds; // random seeds to decorrelate axes

    private void Awake()
    {
        // Start camera at target
        if (cameraPosition != null)
            transform.position = cameraPosition.position;

        // Ensure a Volume exists (emulates QuickVolume if none provided)
        if (volume == null)
        {
            volume = gameObject.GetComponent<Volume>();
            if (volume == null)
            {
                volume = gameObject.AddComponent<Volume>();
                createdRuntimeVolume = true;
            }
        }

        // Configure as a global, high-priority volume (like QuickVolume’s priority = 100f)
        volume.isGlobal = true;
        volume.priority = 100f;

        // Ensure a profile exists
        if (volume.profile == null)
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        // Get or add Vignette override
        if (!volume.profile.TryGet(out vignette))
            vignette = volume.profile.Add<Vignette>(true);

        // Activate and ensure parameters are overridable by code
        vignette.active = true;
        vignette.intensity.overrideState = true;
        vignette.smoothness.overrideState = true;

        // Seed Perlin noise once
        shakeSeeds = new Vector3(
            Random.Range(0.1f, 1000f),
            Random.Range(0.1f, 1000f),
            Random.Range(0.1f, 1000f)
        );
    }

    private void Update()
    {
        // --- Vignette intensity (PPv2 "pulse" adapted to URP) ---
        if (vignette != null)
        {
            float intensity = baseIntensity;
            if (useLifePoints)
                intensity -= Mathf.Max(0f, PlayerMovement.lifePoints) / Mathf.Max(0.0001f, lifeScale);
            vignette.intensity.value = Mathf.Clamp01(intensity);
        }

        // --- Axis-restricted follow with bounds (your original logic) ---
        if (cameraPosition == null) return;

        Vector3 pos = transform.position;

        if (cameraPosition.position.z >= 12f && cameraPosition.position.z <= 130f)
        {
            float z = Mathf.Lerp(pos.z, cameraPosition.position.z, Time.deltaTime * 5f);
            pos = new Vector3(pos.x, pos.y, z);
        }

        if (cameraPosition.position.x >= 22f && cameraPosition.position.x <= 125f)
        {
            float x = Mathf.Lerp(pos.x, cameraPosition.position.x, Time.deltaTime * 5f);
            pos = new Vector3(x, pos.y, pos.z);
        }

        // --- Screen shake update and application ---
        if (shakeTimer < shakeDuration && shakeDuration > 0f && shakeAmplitude > 0f)
        {
            shakeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(shakeTimer / shakeDuration);

            // Ease out (strong at start, fades to zero)
            float falloff = 1f - Mathf.SmoothStep(0f, 1f, t);

            // Perlin-based offsets in [-0.5, 0.5], then scaled
            float time = Time.time * shakeFrequency;
            float nx = Mathf.PerlinNoise(shakeSeeds.x, time) - 0.5f;
            float nz = Mathf.PerlinNoise(shakeSeeds.z, time + 17.123f) - 0.5f;
            float ny = includeY ? (Mathf.PerlinNoise(shakeSeeds.y, time + 31.987f) - 0.5f) : 0f;

            shakeOffset.x = nx * maxShakeXZ.x * shakeAmplitude * falloff * 2f;
            shakeOffset.z = nz * maxShakeXZ.y * shakeAmplitude * falloff * 2f;
            shakeOffset.y = includeY ? ny * maxShakeY * shakeAmplitude * falloff * 2f : 0f;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }

        // Apply the final position with shake
        transform.position = pos + shakeOffset;
    }

    private void OnDestroy()
    {
        // Clean up only what we created (closest URP equivalent to RuntimeUtilities.DestroyVolume)
        if (createdRuntimeVolume && volume != null)
        {
            if (volume.profile != null)
            {
                // Destroy created profile asset instance
                Destroy(volume.profile);
                volume.profile = null;
            }
            Destroy(volume);
            volume = null;
        }
    }

    /// <summary>
    /// Triggers a one-shot screen shake.
    /// amplitude: 0..1 typical (can exceed 1 if you want stronger)
    /// duration: seconds the shake lasts
    /// frequency: how "buzzy" the shake feels (default ~18 Hz)
    /// </summary>
    public void Shake(float amplitude, float duration, float frequency = 18f)
    {
        shakeAmplitude = Mathf.Max(0f, amplitude);
        shakeDuration = Mathf.Max(0f, duration);
        shakeFrequency = Mathf.Max(0.01f, frequency);
        shakeTimer = 0f; // restart
    }

    /// <summary>
    /// Convenience "trauma" style shake: passes amplitude proportional to impact.
    /// e.g., ShakeTrauma(velocityDelta, 30f) where velocityDelta ~ 0..1
    /// </summary>
    public void ShakeTrauma(float trauma, float duration = 0.25f, float frequency = 18f)
    {
        // Square trauma for punchy feel (small hits are subtle, big hits are big)
        Shake(Mathf.Clamp01(trauma) * Mathf.Clamp01(trauma), duration, frequency);
    }
}
    