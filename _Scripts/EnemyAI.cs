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
    public Transform Player;

    private float WaitTime;
    private float DistanceFromPlayer;
    private float NextShotTime;
    private int CurrentPointIndex;

    //Variables for wandering behavior
    private int[] direction;
    private Vector3 TargetDirection;
    private float DistanceModifierX;
    private float DistanceModifierZ;

    Transform EnemyTransform;
    Transform PlayerTransform;
    Rigidbody EnemyRb;

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        //Debug.Log(PatrolPoint);
        direction[0] = 0;//Up
        direction[1] = 1;//Right
        direction[2] = 2;//Down
        direction[3] = 3;//Left
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
        {   //if the player goes far away, return to wandering
            //patrol(); //patrol replaced by wandering
            wandering(direction[CurrentPointIndex]);
            Debug.Log(direction);
        }
    }

    private void chase()
    {
        //Debug.Log("Is chasing");
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

    private void wandering(int dir)
    {
        //move in one direction
        if (dir == 0)//Up
        {
            DistanceModifierZ = 10;
            DistanceModifierX = 0;
        }
        if (dir == 1)//Right
        {
            DistanceModifierZ = 0;
            DistanceModifierX = 10;
        }
        if (dir == 2)//Down
        {
            DistanceModifierZ = -10;
            DistanceModifierX = 0;
        }
        if (dir == 3)//Left
        {
            DistanceModifierZ = 0;
            DistanceModifierX = -10;
        }
        TargetDirection = new Vector3(transform.position.x + DistanceModifierX, transform.position.y, transform.position.z + DistanceModifierZ);
        transform.position = Vector3.MoveTowards(transform.position, TargetDirection, EnemySpeed * Time.deltaTime);

        //wait 
        StartCoroutine(WaitBeforeNextPoint());
        //if collision move to the other direction 
        if (CurrentPointIndex + 1 >= 4)
        {
            CurrentPointIndex = 0;
        }
        else
        {
            CurrentPointIndex++;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            CurrentPointIndex++;
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

            Debug.Log("PatrolPointAchieved");
            //Small pause before moving to next point
            StartCoroutine(WaitBeforeNextPoint());
        }
    }

    private IEnumerator WaitBeforeNextPoint()
    {
        yield return new WaitForSeconds(2f); // Wait 1 second
    }



}
