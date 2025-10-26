using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int health = 3;
    static public bool IsDeath;
    public GameObject hitParticles;
    private void Start()
    {
        IsDeath = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
       if (collision.gameObject.CompareTag("Projectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            if (projectile.Parent.tag != gameObject.tag) {
                Instantiate(hitParticles, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), Quaternion.identity);
                health -= projectile.Damage;
                if (health <= 0) {
                    if (gameObject.tag == "Player")
                    {
                        IsDeath = true;
                        Destroy(gameObject);
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
                Destroy(collision.gameObject);
            }
        }  
    }
}
