using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    //Variables to set up the enemy
    [SerializeField] private float EnemySpeed;
    [SerializeField] private float MinimumDistance;
    [SerializeField] private float SafetyDistance;
    [SerializeField] private float TimeBetweenShots;
    [SerializeField] private GameObject Projectiles;

    //Variables to Set up targets  
    [SerializeField] Transform[] PatrolPoint;
    private Transform Player;

    private float WaitTime;
    private float DistanceFromPlayer;
    private float NextShotTime;
    private int CurrentPointIndex;

    Transform EnemyTransform;
    Transform PlayerTransform;
    Rigidbody EnemyRb;

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        Debug.Log(PatrolPoint);
    }

    // Update is called once per frame
    void Update()
    {
        DistanceFromPlayer = Vector3.Distance(transform.position, Player.position);
        //Debug.Log(DistanceFromPlayer);

        if (DistanceFromPlayer < MinimumDistance)
        {
            //chase if the player is too close to the enemy
            chase();

        }
        else
        {   //if the player goes far away, return to patrol
            patrol();
        }
    }

    private void chase()
    {
        Debug.Log("Is chasing");
        if (Vector3.Distance(transform.position, Player.position) > SafetyDistance)
        {
            //Chase the player and stop to choose at the safety distance
            transform.position = Vector3.MoveTowards(transform.position, Player.position, EnemySpeed * Time.deltaTime);
        }
        else
        {
            shoot();
        }
    }

    private void shoot()
    {
        if (Time.time > NextShotTime)
        {
            Debug.Log("Is Shooting");
            Instantiate(Projectiles, transform.position, Quaternion.identity);
            NextShotTime = Time.time + TimeBetweenShots;
        }
    }

    private void patrol()
    {
        Debug.Log("Is patrolling");
        // Distance to current patrol point
        float distanceToPoint = Vector3.Distance(transform.position, PatrolPoint[CurrentPointIndex].position);

        if (distanceToPoint > 0.2f) // Allow some margin instead of exact position match
        {
            Debug.Log(distanceToPoint);
            transform.position = Vector3.MoveTowards(transform.position, PatrolPoint[CurrentPointIndex].position, EnemySpeed * Time.deltaTime);
        }
        else
        {
            // Move to next patrol point
            if (CurrentPointIndex + 1 >= PatrolPoint.Length)
            {
                CurrentPointIndex = 0;
            }
            else
            {
                CurrentPointIndex++;
            }

            // Optional: small pause before moving to next point
            // StartCoroutine(WaitBeforeNextPoint());
        }
    }

    

}
