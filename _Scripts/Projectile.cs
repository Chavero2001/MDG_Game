using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Vector3 TargetPosition;
    public float ProjectileSpeed;
    public float Damage = 1.0f;

    public GameObject Target;

    //Tag for the projectile to search
    [SerializeField] private string Tag;
    [SerializeField] private float ModifierX;
    [SerializeField] private float ModifierZ;
    //[SerializeField] private string QuotedTag;//Just for FindGameObject to work
    private void Start()
    {
        Target = GameObject.FindGameObjectWithTag(Tag);
        TargetPosition = new Vector3(Target.transform.position.x* ModifierX, Target.transform.position.y, Target.transform.position.z* ModifierZ);

        Invoke("Destroy", 2.0f);
    }

    private void Destroy() {
        Destroy(gameObject);
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, TargetPosition, ProjectileSpeed * Time.deltaTime);

        /*if (transform.position == TargetPosition)
        {
            Destroy(gameObject);
        }*/
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == Tag)
        {
            Destroy(gameObject);
        }
    }
    
}
