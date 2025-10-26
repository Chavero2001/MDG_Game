using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerCamera : MonoBehaviour
{
    [Header("Follow target")]
    public Transform cameraPosition;

    [Header("Volume (optional)")]
    public Volume volume;

    [Header("Vignette control")]
    [Range(0f, 1f)] public float baseIntensity = 0.6f;
    public bool useLifePoints = true;
    public float lifeScale = 20f;

    private Vignette vignette;
    private bool createdRuntimeVolume = false;

    // --- Simple Screen Shake state ---
    private float shakeTime;          // time remaining
    private float shakeDuration;      // total duration
    private float shakeMagnitude;     // max displacement (units)
    private float shakeFrequency;     // noise speed
    private Vector2 shakeSeed;        // random seed so shakes look different

    private void Awake()
    {
        if (cameraPosition != null)
            transform.position = cameraPosition.position;

        if (volume == null)
        {
            volume = gameObject.GetComponent<Volume>();
            if (volume == null)
            {
                volume = gameObject.AddComponent<Volume>();
                createdRuntimeVolume = true;
            }
        }

        volume.isGlobal = true;
        volume.priority = 100f;

        if (volume.profile == null)
            volume.profile = ScriptableObject.CreateInstance<VolumeProfile>();

        if (!volume.profile.TryGet(out vignette))
            vignette = volume.profile.Add<Vignette>(true);

        vignette.active = true;
        vignette.intensity.overrideState = true;
        vignette.smoothness.overrideState = true;
    }

    private void Update()
    {
        // --- Vignette intensity ---
        if (vignette != null)
        {
            float intensity = baseIntensity;
            if (useLifePoints)
                intensity -= Mathf.Max(0f, PlayerMovement.lifePoints) / Mathf.Max(0.0001f, lifeScale);
            vignette.intensity.value = Mathf.Clamp01(intensity);
        }

        // --- Follow with axis restrictions ---
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

        // --- Apply simple screen shake on X/Z ---
        if (shakeTime > 0f)
        {
            // decay (ease out)
            float t = 1f - (shakeTime / Mathf.Max(0.0001f, shakeDuration));
            float decay = 1f - t; // linear; change to (1-t)*(1-t) for faster falloff

            float time = Time.unscaledTime * shakeFrequency;
            // centered perlin in [-1,1]
            float nx = (Mathf.PerlinNoise(shakeSeed.x, time) - 0.5f) * 2f;
            float nz = (Mathf.PerlinNoise(shakeSeed.y, time + 100f) - 0.5f) * 2f;

            Vector3 shakeOffset = new Vector3(nx, 0f, nz) * (shakeMagnitude * decay);
            pos += shakeOffset;

            shakeTime -= Time.unscaledDeltaTime;
        }

        transform.position = pos;
    }

    private void OnDestroy()
    {
        if (createdRuntimeVolume && volume != null)
        {
            if (volume.profile != null)
            {
                Destroy(volume.profile);
                volume.profile = null;
            }
            Destroy(volume);
            volume = null;
        }
    }

    /// <summary>
    /// Triggers a simple camera screen shake.
    /// </summary>
    /// <param name="duration">How long the shake lasts (seconds).</param>
    /// <param name="magnitude">How strong the shake is (world units).</param>
    /// <param name="frequency">How fast it wiggles.</param>
    public void Shake(float duration = 0.2f, float magnitude = 0.15f, float frequency = 25f)
    {
        shakeDuration = Mathf.Max(0.001f, duration);
        shakeTime = shakeDuration;
        shakeMagnitude = Mathf.Max(0f, magnitude);
        shakeFrequency = Mathf.Max(0f, frequency);
        shakeSeed = new Vector2(Random.value * 1000f, Random.value * 1000f);
    }
}
