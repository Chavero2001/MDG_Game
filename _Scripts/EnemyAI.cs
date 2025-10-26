using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    // Enemy setup
    public float EnemySpeed; // will pass to towards the player
    [SerializeField] private float MinimumDistance;
    [SerializeField] private float SafetyDistance;
    public float TimeBetweenShots; // will pass to towards the player
    [SerializeField] public GameObject Projectiles;
    [SerializeField] private float RotationSpeed = 30f;

    public enum EnemyType {
        OfficeWorker,
        BlueCollarWorker,
        GameDev
    }

    public EnemyType enemyType;

    // Targets
    private Transform Player;

    private float DistanceFromPlayer;
    private float NextShotTime;
    private int CurrentPointIndex;

    // Wandering behavior
    private int[] direction = new int[4];
    private Vector3 TargetDirection;
    private bool HasTarget;
    private bool IsWaiting;

    // --- NEW: Wander bounds (XZ plane) ---
    [Header("Wander Bounds (XZ)")]
    [SerializeField] private float minX = 22f;
    [SerializeField] private float maxX = 125f;
    [SerializeField] private float minZ = 12f;
    [SerializeField] private float maxZ = 130f;
    [SerializeField] private float edgePadding = 0.5f; // keep a small buffer from edges

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        direction[0] = 0; // Up
        direction[1] = 1; // Right
        direction[2] = 2; // Down
        direction[3] = 3; // Left

        // Safety: clamp spawn position in case it starts outside
        transform.position = ClampToBounds(transform.position);
    }

    void Update()
    {
        DistanceFromPlayer = Vector3.Distance(transform.position, Player.position);

        if (DistanceFromPlayer < MinimumDistance)
        {
            chase();
        }
        else
        {
            if (!IsWaiting)
                wandering(direction[CurrentPointIndex]);

            // Optional: hard clamp each frame (physics or other forces can push you out)
            transform.position = ClampToBounds(transform.position);
        }
    }

    private void chase()
    {
        if (Vector3.Distance(transform.position, Player.position) > SafetyDistance)
        {
            Vector3 moveDir = (Player.position - transform.position).normalized;

            // Rotate toward player
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Mathf.Clamp01(RotationSpeed * Time.deltaTime)
                );
            }

            transform.position += moveDir * EnemySpeed * Time.deltaTime;
        }
        else
        {
            shoot();
            Vector3 moveDir = (Player.position - transform.position).normalized;

            // Rotate toward player
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    Mathf.Clamp01(RotationSpeed * Time.deltaTime)
                );
            }
        }
    }

    // --- REPLACED: bounded wandering with typo fix (transform, not trnasform) ---
    private void wandering(int dir)
    {
        if (!HasTarget)
        {
            Vector3 step = dir switch
            {
                0 => Vector3.forward * 10f,
                1 => Vector3.right * 10f,
                2 => Vector3.back * 10f,
                3 => Vector3.left * 10f,
                _ => Vector3.zero
            };

            Vector3 candidate = transform.position + step;

            // If candidate is out of bounds (considering padding), try the opposite direction.
            if (!InsideBounds(candidate, edgePadding))
            {
                Vector3 opposite = transform.position - step;
                candidate = InsideBounds(opposite, edgePadding) ? opposite : ClampToBounds(candidate);
            }

            TargetDirection = ClampToBounds(candidate);
            HasTarget = true;
        }

        Vector3 moveDir = (TargetDirection - transform.position).normalized;

        // Rotate toward movement direction
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                Mathf.Clamp01(RotationSpeed * Time.deltaTime)
            );
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            TargetDirection,
            EnemySpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, TargetDirection) < 0.2f)
        {
            HasTarget = false;
            StartCoroutine(WaitAndChooseNextDirection());
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            CurrentPointIndex = (CurrentPointIndex + 1) % 4;
            HasTarget = false; // force a new pick next frame
        }
    }

    private void shoot()
    {
        if (Time.time > NextShotTime)
        {
            // Always rotate to face the player, even while shooting
            Vector3 directionToPlayer = (Player.position - transform.position).normalized;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Mathf.Clamp01(RotationSpeed * Time.deltaTime)
                );
            }

            // Calculate a spawn point slightly in front of the enemy
            float spawnDistance = 1.0f; // adjust depending on enemy size
            Vector3 spawnPosition = transform.position + transform.forward * spawnDistance;

            // Instantiate the projectile facing the same direction as the enemy
            GameObject projectileInstance = Instantiate(Projectiles, spawnPosition, transform.rotation);

            // Initialize the projectile
            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            projectile.Init("Player", gameObject);

            NextShotTime = Time.time + TimeBetweenShots;
        }
    }

    private IEnumerator WaitAndChooseNextDirection()
    {
        IsWaiting = true;
        yield return new WaitForSeconds(2f);
        CurrentPointIndex = Random.Range(0, 4); // optional: randomize direction
        IsWaiting = false;
    }

    private bool InsideBounds(Vector3 p, float padding = 0f)
    {
        return p.x >= (minX + padding) && p.x <= (maxX - padding) &&
               p.z >= (minZ + padding) && p.z <= (maxZ - padding);
    }

    private Vector3 ClampToBounds(Vector3 p)
    {
        float x = Mathf.Clamp(p.x, minX, maxX);
        float z = Mathf.Clamp(p.z, minZ, maxZ);
        return new Vector3(x, p.y, z);
    }
}
