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
    [SerializeField] private Transform Player;
    [SerializeField] Transform[] PatrolPoint;

    private float WaitTime;
    private float DistanceFromPlayer;
    private float NextShotTime;
    private int CurrentPointIndex;

    Transform EnemyTransform;
    Transform PlayerTransform;
    Rigidbody EnemyRb;

   // Update is called once per frame
    void Update()
    {
        DistanceFromPlayer = Vector3.Distance(transform.position, Player.position);

        if (DistanceFromPlayer < MinimumDistance)
        {
            escape();

        }
        else
        {
            patrol();
        }
    }

    private void escape()
    {
        if (Vector3.Distance(transform.position, Player.position) < SafetyDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, Player.position, -EnemySpeed * Time.deltaTime);
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
            Instantiate(Projectiles, transform.position, Quaternion.identity);
            NextShotTime = Time.time + TimeBetweenShots;
        }
    }
    
    private void patrol()
    {
        if (transform.position != PatrolPoint[CurrentPointIndex].position)
        {
            transform.position = Vector2.MoveTowards(transform.position, PatrolPoint[CurrentPointIndex].position, EnemySpeed * Time.deltaTime);
            //Debug.Log("First Patrol");
            Debug.Log(CurrentPointIndex);
            Debug.Log(PatrolPoint.Length);
        }
        else
        {

            Debug.Log(CurrentPointIndex);
            if (CurrentPointIndex + 1 == PatrolPoint.Length)
            {
                CurrentPointIndex = 0;
            }
            else
            {
                CurrentPointIndex++;
            }
        }
    }
}
