using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    private float spawnDistance = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 globalSpawnPosition = transform.position + transform.forward * spawnDistance ;
            Quaternion spawnRotation = Quaternion.identity;
            Instantiate(projectilePrefab, globalSpawnPosition, spawnRotation);

        }
    }
}
