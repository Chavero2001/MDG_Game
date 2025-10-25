using UnityEngine;

public class Invisible : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public int radius=5;
    void Start()
    {
        GameObject cam = GameObject.FindGameObjectWithTag("Player");
        if (cam != null)
        {
            Vector3 camPos = cam.transform.position;

            // Check if x and z are within -5 to +5
            if (Mathf.Abs(camPos.x) <= radius && Mathf.Abs(camPos.z) <= radius)
            {
                // Get the Renderer component
                if (meshRenderer != null)
                {
                    Color color = meshRenderer.material.color;
                    color.a = 0.5f;
                    meshRenderer.material.color = color;
                }
            }
        }
    }
}
