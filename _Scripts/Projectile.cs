using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 Direction;
    private Vector3 TargetPosition;
    private Rigidbody Rb;

    public float ProjectileSpeed = 10f;
    public int Damage = 1;

    public string Tag; // passed from the parent(spawner)
    //THis modifiers are just in case the precistion of the bullet needs to be adjusted
    [SerializeField] private float ModifierX = 0f; 
    [SerializeField] private float ModifierZ = 0f;
    [SerializeField] private float ModifierY = 1f;
    public GameObject Parent;

    private void Start()
    {
        Rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Move projectile in its stored direction
        transform.position += Direction * ProjectileSpeed * Time.deltaTime;
    }

    private void OnCollisionEnter(Collision collider)
    {
        // Destroy projectile if it hits its intended target
        if (collider.collider.CompareTag(Tag))
        {
            PlayerMovement.lifePoints -= 1;
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider collider)
    {
        // Destroy projectile if it hits its intended target
        if (collider.CompareTag(Tag))
        {
            
            Destroy(gameObject);
        }
    }

    public void Init(string TargetTag, GameObject parent)
    {
        Parent = parent;

        if (TargetTag == "Player")
        {
            // Enemy fires toward player
            Tag = "Player";
            GameObject target = GameObject.FindGameObjectWithTag("Player");

            if (target != null)
            {
                TargetPosition = new Vector3(
                    target.transform.position.x + ModifierX,
                    target.transform.position.y + ModifierY,
                    target.transform.position.z + ModifierZ
                );
                Direction = (TargetPosition - transform.position).normalized;
            }
        }
        else if (TargetTag == "Enemy")
        {
            // Player fires in the direction they are facing
            Tag = "Enemy";
            Direction = Parent.transform.forward; // use player's forward direction
        }

        // Automatically destroy after 2 seconds
        Invoke(nameof(SelfDestroy), 2f);
    }

    private void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
