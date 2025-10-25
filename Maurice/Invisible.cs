using UnityEngine;

public class Invisible : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public float radius = 5f;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || meshRenderer == null) return;

        // Distance check (x/z only, ignoring height)
        Vector3 playerPos = player.transform.position;
        Vector3 objPos = transform.position;

        float dx = playerPos.x - objPos.x;
        float dz = playerPos.z - objPos.z;

        if (Mathf.Abs(dx) <= radius && Mathf.Abs(dz) <= radius)
        {
            // Make 50% transparent
            Color c = meshRenderer.material.color;
            c.a = 0.5f;
            meshRenderer.material.color = c;
        }
    }
}
