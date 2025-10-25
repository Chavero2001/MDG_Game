using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public float health = 10.0f;

    private void OnCollisionEnter(Collision collision)
    {
       if (collision.gameObject.CompareTag("Projectile"))
        {
            // TODO: implement health
            Debug.Log("Collided with an enemy!");
            Destroy(gameObject);
        }  
    }
}
