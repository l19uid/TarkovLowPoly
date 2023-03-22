using System;
using UnityEngine;

public class MovePlayer : MonoBehaviour
{
    float playerHeight = 2f;

    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float maxSpeed = 10f;
    [SerializeField] float airMultiplier = 0.4f;
    float movementMultiplier = 10f;

    bool isCrouching = false;
    bool isSprinting = false;

    [Header("Jumping")]
    public float jumpForce = 5f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

    Vector2 input;

    [Header("Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform headCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundDistance = 0.2f;

    public bool isGrounded;
    public bool canStand;

    Vector3 moveDirection;
    Vector3 slopeMoveDirection;
    Rigidbody rb;

    RaycastHit slopeHit;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        ManageInput();
        ManageMovement();

        if (Input.GetKeyDown(jumpKey) && IsGrounded())
            Jump();
        
        if(!IsGrounded())
            rb.velocity = new Vector3(rb.velocity.x,-10,rb.velocity.z);
    }

    void ManageInput()
    {
        input.x = Input.GetAxisRaw("Vertical");
        input.y = Input.GetAxisRaw("Horizontal");
        
        if(Input.GetKey(KeyCode.W) && input.x > 0 && Input.GetKey(KeyCode.LeftShift))
            isSprinting = true;
        else 
            isSprinting = false;
        
        moveDirection = orientation.forward * input.x + orientation.right * input.y;
    }

    void Jump()
    {
        rb.velocity = Vector3.up * jumpForce;
    }

    void ManageMovement()
    {
        if(isSprinting)
            rb.velocity = (moveDirection.normalized * moveSpeed * 1.5f);
        else 
            rb.velocity = (moveDirection.normalized * moveSpeed);
    }

    void OnCollisionStay(Collision other)
    {
        if (other.gameObject.tag == "Ground" || IsGrounded())
        {
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundDistance);
    }
}