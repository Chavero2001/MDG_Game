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


    private Vignette vignette;
    private bool createdRuntimeVolume = false;          // Track if we created one to clean up later

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
    }

    private void Update()
    {
        // --- Vignette intensity (PPv2 "pulse" adapted to URP) ---
        if (vignette != null)
        {
            float intensity = baseIntensity;
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

        transform.position = pos;
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
}
