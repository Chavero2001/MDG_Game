using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int health = 3;
    public GameObject hitParticles;
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
                        
                        Destroy(gameObject);
                    }
                    else
                    {
                        GameManager.Instance.AddEnemyDestroyed();
                        Destroy(gameObject);
                    }
                }
                Destroy(collision.gameObject);
            }
        }  
    }
}
