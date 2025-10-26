using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    [SerializeField]private float spawnDistance = 2.0f;
    public AudioSource audioSource;
    public float fireCooldown = 0.4f;

    public bool CanFire = true;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1") && CanFire)
        {
            audioSource.Play();
            Vector3 globalSpawnPosition = transform.position + transform.forward * spawnDistance ;
            globalSpawnPosition = new Vector3(globalSpawnPosition.x, globalSpawnPosition.y + 1, globalSpawnPosition.z);
            Quaternion spawnRotation = Quaternion.identity;
            GameObject projectileInstance = Instantiate(projectilePrefab, globalSpawnPosition, spawnRotation);
            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            projectile.Init("Enemy", gameObject);

            CanFire = false;
            Invoke(nameof(enableFire), fireCooldown);
        }
    }

    private void enableFire() {
        CanFire = true;
    }
}
