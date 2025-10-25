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
<<<<<<< HEAD
=======
        //Debug.Log(horizontalInput);

>>>>>>> 9e49fcea2d7e4aef491ba0c94c1ab82aaa42df48
        Vector3 inputDir = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 moveDir = transform.TransformDirection(inputDir);
        Vector3 movementVelocity = moveDir * moveSpeed;
        rb.linearVelocity = movementVelocity;
    }
}
