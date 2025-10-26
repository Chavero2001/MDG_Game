using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]    
    public float moveSpeed;
    public Transform orientation;
    public Camera playerCamera;
    public Vector3 LookDir;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    public PlayerCamera playerCamera1;
    Rigidbody rb;
    private float regenTimer=0;
    private float dashTimer = 0f;
    private float dashSpeed = 1f;
    static public float lifePoints = 5f;
    private float previousLife = lifePoints;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if(previousLife != lifePoints)
        {
            playerCamera1.Shake(0.25f, 0.2f, 30f);
            previousLife = lifePoints;
        }
        if (lifePoints < 5)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer > 2)
            {                lifePoints += 1;
                previousLife = lifePoints;
                regenTimer = 0;
            }
        }
        // Rotation
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance)) {
            Vector3 hitPoint = ray.GetPoint(distance);

            // Look direction (ignore height)
            LookDir = hitPoint - rb.position;
            LookDir.y = 0;

            if (LookDir.sqrMagnitude > 0.001f) {
                Quaternion targetRotation = Quaternion.LookRotation(LookDir);
                rb.MoveRotation(targetRotation); // smoother than setting rb.rotation directly
            }
        }

        // Movement
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 moveDir = inputDir;//transform.TransformDirection(inputDir);
        if (Input.GetButtonDown("Jump") && dashTimer<=-0.5)
        {
            smoke.SetActive(true);
            dashSpeed = 5f;
            dashTimer = 0.035f;
        }
        if(dashTimer <= 0)
        {
            dashSpeed = 1f;
        }
        dashTimer -= Time.deltaTime;
        Vector3 movementVelocity = moveDir * moveSpeed*dashSpeed;
        rb.linearVelocity = movementVelocity;
    }
}
