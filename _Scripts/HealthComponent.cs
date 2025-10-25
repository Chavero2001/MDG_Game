using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int health = 3;

    private void OnCollisionEnter(Collision collision)
    {
       if (collision.gameObject.CompareTag("Projectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            if (projectile.Parent != gameObject) {
                health -= projectile.Damage;
                if (health <= 0) {
                    Destroy(gameObject);
                }
                Destroy(collision.gameObject);
            }
        }  
    }
}
