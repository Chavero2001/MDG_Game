using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int health = 3;

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collide");
       if (collision.gameObject.CompareTag("Projectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            Debug.Log("Hit");
            if (projectile.Parent != gameObject) {
                health -= projectile.Damage;
                if (health <= 0) {
                    Debug.Log("Die");
                    Destroy(gameObject);
                }
                Destroy(collision.gameObject);
            }
        }  
    }
}
