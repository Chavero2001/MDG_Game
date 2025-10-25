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
    private Transform Player;

    private float WaitTime;
    private float DistanceFromPlayer;
    private float NextShotTime;
    private int CurrentPointIndex;

    //Variables for wandering behavior
    private int[] direction = new int[4];
    private Vector3 TargetDirection;
    private bool HasTarget;
    private bool IsWaiting;
    private float DistanceModifierX;
    private float DistanceModifierZ;

    Transform EnemyTransform;
    Transform PlayerTransform;
    Rigidbody EnemyRb;

    private void Start()
    {
        Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
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
        if (!HasTarget)
        {
            if (dir == 0) TargetDirection = transform.position + Vector3.forward * 10;
            if (dir == 1) TargetDirection = transform.position + Vector3.right * 10;
            if (dir == 2) TargetDirection = transform.position + Vector3.back * 10;
            if (dir == 3) TargetDirection = transform.position + Vector3.left * 10;
            HasTarget = true;
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
        if (collision.gameObject.tag == "Wall")
        {
            CurrentPointIndex++;
        }
    }

    private void shoot()
    {
        if (Time.time > NextShotTime)
        {
            GameObject projectileInstance = Instantiate(Projectiles, transform.position, Quaternion.identity);
            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            projectile.Init("Player", gameObject);
            NextShotTime = Time.time + TimeBetweenShots;
        }
    }

    private IEnumerator WaitBeforeNextPoint()
    {
        yield return new WaitForSeconds(2f); // Wait 1 second
    }
    private IEnumerator WaitAndChooseNextDirection()
    {
        IsWaiting = true;
        yield return new WaitForSeconds(2f);
        CurrentPointIndex = (CurrentPointIndex + 1) % 4;
        IsWaiting = false;
    }


}
