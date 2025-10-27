using UnityEngine;
using System.Collections;
public class Projectile : MonoBehaviour
{
    private Vector3 Direction;
    private Vector3 TargetPosition;
    private Rigidbody Rb;
    private Collider col;
    public float ProjectileSpeed = 10f;
    public int Damage = 1;

    public string Tag; // passed from the parent(spawner)
    //THis modifiers are just in case the precistion of the bullet needs to be adjusted
    [SerializeField] private float ModifierX = 0f; 
    [SerializeField] private float ModifierZ = 0f;
    [SerializeField] private float ModifierY = 1f;
    [SerializeField] private float rotationSpeed = 0f;
    public GameObject Parent;

    private void Start()
    {
        col = GetComponent<Collider>();
        Rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        // Move projectile in its stored direction
        transform.position += Direction * ProjectileSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

    }

    private void OnCollisionEnter(Collision collider)
    {
        // Destroy projectile if it hits its intended target
        if (collider.collider.CompareTag(Tag))
        {
            PlayerMovement.lifePoints -= Tag == "Player"?1:0;
            StartCoroutine(SelfDestructSequence());
        }
       
    }
    private void OnTriggerEnter(Collider collider)
    {
        // Destroy projectile if it hits its intended target
        if (collider.CompareTag(Tag))
        {
            PlayerMovement.lifePoints -= Tag == "Player" ? 1 : 0;
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
        Invoke(nameof(BeginSelfDestruct), 4f);
    }

    private void BeginSelfDestruct()
    {
        StartCoroutine(SelfDestructSequence());
    }
    private IEnumerator SelfDestructSequence()
    {
        // disable collider so it can’t hit anything else
        if (col != null)
            col.enabled = false;

        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 initialScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            yield return null;
        }

        Destroy(gameObject);
    }

}
