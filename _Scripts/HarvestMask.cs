using UnityEngine;
using System.Collections.Generic;

public class HarvestMask : MonoBehaviour
{
    public GameObject ClosestEnemy;
    [SerializeField] public GameObject Arrow;
    [SerializeField] public GameObject Player;

    private List<GameObject> EnemiesInRange = new();
    private SpriteRenderer arrowSpriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arrowSpriteRenderer = Arrow.GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (ClosestEnemy != null)  {
            arrowSpriteRenderer.enabled = true;
            Vector3 directionToClosestEnemy = (ClosestEnemy.transform.position - Player.transform.position).normalized * 1.2f;
            Arrow.transform.position = Player.transform.position + directionToClosestEnemy;
            Quaternion targetRotation = Quaternion.LookRotation(directionToClosestEnemy);
            Arrow.transform.rotation = targetRotation;
            Vector3 currentEuler = Arrow.transform.eulerAngles;
            currentEuler.x = 90f; // Force Y to 90
            Arrow.transform.eulerAngles = currentEuler; 
        } else {
            arrowSpriteRenderer.enabled = false;
        }

        if (Input.GetButtonDown("Fire2")) {
            if (ClosestEnemy != null) {
                // Teleport to the Closest Enemy
                Player.transform.position = ClosestEnemy.transform.position;

                // Transfer the stats
                EnemyAI enemyAi = ClosestEnemy.GetComponent<EnemyAI>();
                GameObject enemyProjectile = enemyAi.Projectiles;
                PlayerAttack playerAttack = Player.GetComponent<PlayerAttack>();
                playerAttack.projectilePrefab = enemyProjectile;

                // Destroy the Closest Enemy
                EnemiesInRange.Remove(ClosestEnemy);
                Destroy(ClosestEnemy);
                ClosestEnemy = null;
            }
        }
    }



    private void OnTriggerEnter(Collider collider) {
        if (collider.CompareTag("Enemy")) {
            EnemiesInRange.Add(collider.gameObject);
        }
        ClosestEnemy = GetClosestEnemy();
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.CompareTag("Enemy")) {
            EnemiesInRange.Remove(collider.gameObject);
        }
        ClosestEnemy = GetClosestEnemy();
    }

    private GameObject GetClosestEnemy() {
        float closetDistance = Mathf.Infinity;
        GameObject closest = null;

        for (int i = 0 ; i < EnemiesInRange.Count ; i++) {
            if (EnemiesInRange[i] == null)
                continue;
            
            float distance = (transform.position - EnemiesInRange[i].transform.position).sqrMagnitude;
            if (distance < closetDistance) {
                closetDistance = distance;
                closest = EnemiesInRange[i];
            }
        }

        return closest;
    }
}
