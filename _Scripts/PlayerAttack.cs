using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    [SerializeField]private float spawnDistance = 2.0f;
    public AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            audioSource.Play();
            Vector3 globalSpawnPosition = transform.position + transform.forward * spawnDistance ;
            Quaternion spawnRotation = Quaternion.identity;
            GameObject projectileInstance = Instantiate(projectilePrefab, globalSpawnPosition, spawnRotation);
            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            projectile.Init("Enemy", gameObject);
        }
    }
}
