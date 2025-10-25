using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public float health = 3.0f;

    private void OnCollisionEnter(Collision collision)
    {
       if (collision.gameObject.CompareTag("Projectile"))
        {
            Projectile projectile = collision.gameObject.GetComponent<Projectile>();
            health -= projectile.Damage;
            Debug.Log(health);
            if (health <= 0.0f) {
                Destroy(gameObject);
            }
            Destroy(collision.gameObject);
        }  
    }
}
