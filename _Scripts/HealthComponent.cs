using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public int health = 3;
    static public bool IsDeath;

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
