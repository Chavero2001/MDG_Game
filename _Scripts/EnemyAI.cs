using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    // Enemy setup
    public float EnemySpeed; //will pass to towards the player
    [SerializeField] private float MinimumDistance;
    [SerializeField] private float SafetyDistance;
    public float TimeBetweenShots; //will pass to towards the player
    [SerializeField] private GameObject Projectiles;
    [SerializeField] private float RotationSpeed = 5f; 

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

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        direction[0] = 0; // Up
        direction[1] = 1; // Right
        direction[2] = 2; // Down
        direction[3] = 3; // Left
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
            wandering(direction[CurrentPointIndex]);
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
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
            }

            transform.position += moveDir * EnemySpeed * Time.deltaTime;
        }
        else
        {
            shoot();
        }
    }

    private void wandering(int dir)
    {
        if (!HasTarget)
        {
            if (dir == 0) TargetDirection = transform.position + Vector3.forward * 10;
            if (dir == 1) TargetDirection = transform.position + Vector3.right * 10;
            if (dir == 2) TargetDirection = transform.position + Vector3.back * 10;
            if (dir == 3) TargetDirection = transform.position + Vector3.left * 10;
            HasTarget = true;
        }

        Vector3 moveDir = (TargetDirection - transform.position).normalized;

        // Rotate toward movement direction
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, RotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, TargetDirection, EnemySpeed * Time.deltaTime);

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
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
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
}
