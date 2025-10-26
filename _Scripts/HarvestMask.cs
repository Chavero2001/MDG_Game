using UnityEngine;
using System.Collections.Generic;

public class HarvestMask : MonoBehaviour
{
    public GameObject ClosestEnemy;
    [SerializeField] public GameObject Arrow;
    [SerializeField] public GameObject Player;
    [SerializeField] public GameObject MaskAttachement;
    [SerializeField] public GameObject OfficeWorkerMask;
    [SerializeField] public GameObject BlueCollarWorkerMask;
    [SerializeField] private GameObject OfficeWorkerPrefab;
    [SerializeField] private GameObject BlueCollarWorkerPrefab;
    public AudioSource audioSource;
    private List<GameObject> EnemiesInRange = new();
    private SpriteRenderer arrowSpriteRenderer;
    private PlayerMovement playerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arrowSpriteRenderer = Arrow.GetComponent<SpriteRenderer>();
        playerMovement = Player.GetComponent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
        // Attach the masks to the face (terrible copy paste code lol)
        OfficeWorkerMask.transform.rotation = Quaternion.LookRotation(playerMovement.LookDir.normalized);
        OfficeWorkerMask.transform.position = MaskAttachement.transform.position + playerMovement.LookDir.normalized * 0.2f - new Vector3(0.0f, 0.3f, 0.0f);
        BlueCollarWorkerMask.transform.rotation = Quaternion.LookRotation(playerMovement.LookDir.normalized);
        Vector3 blueCollarMaskCurrentEuler = BlueCollarWorkerMask.transform.eulerAngles;
        blueCollarMaskCurrentEuler.x = -90f;
        BlueCollarWorkerMask.transform.eulerAngles = blueCollarMaskCurrentEuler; 
        BlueCollarWorkerMask.transform.position = MaskAttachement.transform.position + playerMovement.LookDir.normalized * 0.2f - new Vector3(0.0f, 0.3f, 0.0f);
        */
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
                audioSource.Play();
                // Teleport to the Closest Enemy
                Player.transform.position = ClosestEnemy.transform.position;

                //Calls he instance of the GameManager 
                GameManager.Instance.AddEnemyDestroyed();
                GameManager.Instance.AddFacesObtained();

                // Copy the stats
                EnemyAI enemyAi = ClosestEnemy.GetComponent<EnemyAI>();
                GameObject enemyProjectile = enemyAi.Projectiles;
                PlayerAttack playerAttack = Player.GetComponent<PlayerAttack>();
                PlayerMovement playerMovement = Player.GetComponent<PlayerMovement>();
                // Copy the projectile
                playerAttack.projectilePrefab = enemyProjectile;
                // Copy the movement speed
                playerMovement.moveSpeed = enemyAi.EnemySpeed;

                // Make the appropriate mask visible
                HideAllMasks();
                if (enemyAi.enemyType == EnemyAI.EnemyType.OfficeWorker) {
                    MeshRenderer officeWorkerMask = OfficeWorkerMask.GetComponent<MeshRenderer>();
                    officeWorkerMask.enabled = true;
                } else {
                    MeshRenderer blueCollarWorkerMask = BlueCollarWorkerMask.GetComponent<MeshRenderer>();
                    blueCollarWorkerMask.enabled = true;
                }

                // Destroy the Closest Enemy
                EnemiesInRange.Remove(ClosestEnemy);
                Destroy(ClosestEnemy);
                ClosestEnemy = null;
            }
        }
    }


    private void HideAllMasks() {
        MeshRenderer officeWorkerMask = OfficeWorkerMask.GetComponent<MeshRenderer>();
        officeWorkerMask.enabled = false;
        MeshRenderer blueCollarWorkerMask = BlueCollarWorkerMask.GetComponent<MeshRenderer>();
        blueCollarWorkerMask.enabled = false;
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
