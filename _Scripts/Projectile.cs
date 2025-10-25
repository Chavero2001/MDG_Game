using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Vector3 TargetPosition;
    public float ProjectileSpeed;
    public int Damage = 1;

    public GameObject Target;

    private Vector3 Direction; 

    //Tag for the projectile to search
    [SerializeField] public string Tag;
    [SerializeField] private float ModifierX;
    [SerializeField] private float ModifierZ;

    public GameObject Parent;

    private void Start()
    {
    }

    private void Destroy() {
        Destroy(gameObject);
    }

    private void Update()
    {
        //transform.position = Vector3.MoveTowards(transform.position, TargetPosition, ProjectileSpeed * Time.deltaTime);
        if (tag == "Player")//Projectile behavior if the enemy is spawing the projectile
        {
            transform.position += Direction * ProjectileSpeed * Time.deltaTime;
        }
        if (tag == "Enemy")//Projectile if player spawns the projectile
        {
            transform.position += gameObject.transform.position; 
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.tag == Tag)
        {
            Destroy(gameObject);
        }
    }

    public void Init(string tag, GameObject parent) {
        Tag = tag;
        Parent = parent;

        if (Tag != null && Tag != "") {
            Target = GameObject.FindGameObjectWithTag(Tag);
            TargetPosition = new Vector3(Target.transform.position.x* ModifierX, Target.transform.position.y, Target.transform.position.z* ModifierZ);

            Direction = (TargetPosition - transform.position).normalized;
            Invoke("Destroy", 2.0f);
        }
    }
    
}
