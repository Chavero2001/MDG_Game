using UnityEngine;

<<<<<<< HEAD
[RequireComponent(typeof(Renderer))]
public class Invisible : MonoBehaviour
{
    public float radius = 2.5f;

    public Renderer rend;

    [Tooltip("Material slots (indices) that should toggle visibility")]
    public int[] targetMaterialIndices;

    void Awake()
    {
        if (rend == null) rend = GetComponent<Renderer>();

        // Clone only the chosen materials so they are unique to this object
        Material[] mats = rend.materials;
        foreach (int idx in targetMaterialIndices)
        {
            if (idx >= 0 && idx < mats.Length && mats[idx] != null)
            {
                mats[idx] = new Material(mats[idx]);
            }
        }
        rend.materials = mats;
    }

    void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null || rend == null) return;

        // Distance check (XZ plane only)
        Vector3 playerPos = player.transform.position;
        Vector3 objPos = transform.position;
        float dx = playerPos.x - objPos.x;
        float dz = playerPos.z - objPos.z;
        bool inRange = Mathf.Abs(dx) <= radius && Mathf.Abs(dz) <= radius;

        // Apply to each selected index
        Material[] mats = rend.materials;
        foreach (int idx in targetMaterialIndices)
        {
            if (idx >= 0 && idx < mats.Length && mats[idx] != null)
            {
                Material mat = mats[idx];
                string colorProp = mat.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
                Color c = mat.GetColor(colorProp);
                c.a = inRange ? 0.5f : 1f;
                mat.SetColor(colorProp, c);
            }
=======
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
>>>>>>> 02539d8a2dbeef7b1aa6b8cb46d2fed282bf166b
        }
    }
}
