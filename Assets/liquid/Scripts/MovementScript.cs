using System;
using UnityEngine;

public class MovementScript : MonoBehaviour
{
    float playerHeight = 2f;

    [SerializeField] Transform orientation;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 4f;
    [SerializeField] float sprintMultiplier = 1.4f;
    [SerializeField] float airMultiplier = 0.4f;
    [SerializeField] float turningSpeed;
    
    bool isCrouching = false;
    bool isSprinting = false;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Keybinds")]
    [SerializeField] KeyCode jumpKey = KeyCode.Space;
    [SerializeField] KeyCode sprintKey = KeyCode.LeftShift;

    Vector2 input;

    [Header("Detection")]
    [SerializeField] Transform groundCheck;
    [SerializeField] Transform headCheck;
    [SerializeField] LayerMask groundMask;
    [SerializeField] LayerMask wallMask;
    [SerializeField] float groundDistance;

    [SerializeField] Vector3 moveDirection;
    [SerializeField] Vector3 inputDirection;
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
        if(CanWalkSlope())
            rb.velocity = new Vector3(rb.velocity.x,0,rb.velocity.z);
        if(!IsGrounded())
            rb.velocity = new Vector3(rb.velocity.x,gravity,rb.velocity.z);
    }

    void ManageInput()
    {
        input.x = Input.GetAxisRaw("Vertical");
        input.y = Input.GetAxisRaw("Horizontal");
        
        if(Input.GetKey(KeyCode.W) && input.x > 0 && Input.GetKey(KeyCode.LeftShift))
            isSprinting = true;
        else 
            isSprinting = false;
        
        inputDirection = orientation.forward * input.x + orientation.right * input.y;
        
        if(inputDirection != Vector3.zero)
            moveDirection = Vector3.Lerp(moveDirection, inputDirection, Time.deltaTime * turningSpeed);
        else
            moveDirection = Vector3.zero;
    }

    void Jump()
    {
        rb.velocity = Vector3.up * jumpForce * jumpForce;
    }

    void ManageMovement()
    {
        if (isSprinting)
            rb.velocity = (moveDirection.normalized * moveSpeed * sprintMultiplier);
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
        return Physics.CheckSphere(groundCheck.position, groundDistance/2, groundMask);
    }
    
    private bool IsOnSlope()
    {
        if (Physics.Raycast(groundCheck.transform.position,new Vector3(moveDirection.x * .25f,-.05f,moveDirection.z * .25f) , out slopeHit, 1f, groundMask))
        {
            if (slopeHit.normal != Vector3.up)
            {
                float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
                if (angle >= 25f)
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    private bool CanWalkSlope()
    {
        if (Physics.Raycast(groundCheck.transform.position,new Vector3(moveDirection.x * .25f,-.05f,moveDirection.z * .25f) , out slopeHit, .75f, groundMask))
        {
            if (slopeHit.normal != Vector3.up)
            {
                float angle = Vector3.Angle(slopeHit.normal, Vector3.up);
                if (angle >= 50f)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundDistance/2);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + moveDirection.normalized * 2f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + inputDirection.normalized * 2f);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(groundCheck.transform.position,groundCheck.transform.position + new Vector3(moveDirection.x * .25f,-.05f,moveDirection.z * .25f)*2);
    }
}