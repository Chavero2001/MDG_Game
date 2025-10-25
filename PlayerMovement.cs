using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]    
    public float moveSpeed;
    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    
    Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        Debug.Log(horizontalInput);

        Vector3 inputDir = new Vector3(horizontalInput, 0f, verticalInput).normalized;
        Vector3 movementVelocity = inputDir * moveSpeed;
        rb.linearVelocity = movementVelocity;
    }
}
