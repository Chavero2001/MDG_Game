using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Vector3 TargetPosition;
    public float ProjectileSpeed;

    GameObject Target;

    //Tag for the projectile to search
    [SerializeField] private string Tag;
    private void Start()
    {
        Target = GameObject.FindGameObjectWithTag(Tag);
        TargetPosition = Target.transform.position;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, ProjectileSpeed * Time.deltaTime);

        /*if (transform.position == TargetPosition)
        {
            Destroy(gameObject);
        }*/
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Destroy(gameObject);
        }
    }
}
