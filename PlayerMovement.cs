using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]    
    public float moveSpeed;
    public Transform orientation;
    public Camera playerCamera;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    
    Rigidbody rb;
    private float dashTimer = 0f;
    private float dashSpeed = 1f;

    static public float lifePoints = 5f;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        // Rotation
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance)) {
            Vector3 hitPoint = ray.GetPoint(distance);

            // Look direction (ignore height)
            Vector3 lookDir = hitPoint - rb.position;
            lookDir.y = 0;

            if (lookDir.sqrMagnitude > 0.001f) {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);
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
            dashSpeed = 5f;
            dashTimer = 0.1f;
        }
        if(dashTimer <= 0)
        {
            dashSpeed = 1f;
        }
        dashTimer -= Time.deltaTime;
        Vector3 movementVelocity = moveDir * moveSpeed*dashSpeed;
        rb.linearVelocity = movementVelocity;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Projectile"))
        {
            lifePoints -= 1;
        }
    }
}
