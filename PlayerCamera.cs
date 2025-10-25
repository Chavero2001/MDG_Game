using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.Rendering.DebugUI;
public class PlayerCamera : MonoBehaviour
{
    public Transform cameraPosition;
    private Vignette vignette;
    public Volume volume;
    void Start() {
        transform.position = new Vector3(cameraPosition.position.x, cameraPosition.position.y, cameraPosition.position.z);
    }

    void Update()
    {
        vignette.intensity.value = 0.6f-(PlayerMovement.lifePoints/20f);
        if (cameraPosition.position.z >= 12 && cameraPosition.position.z <= 130)
        {
            Vector3 target = new Vector3(transform.position.x, transform.position.y, cameraPosition.position.z);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 5f);
        }

        if (cameraPosition.position.x >= 22 && cameraPosition.position.x <= 125)
        {
            Vector3 target = new Vector3(cameraPosition.position.x, transform.position.y, transform.position.z);
            transform.position = Vector3.Lerp(transform.position, target, Time.deltaTime * 5f);
        }
    }
}
