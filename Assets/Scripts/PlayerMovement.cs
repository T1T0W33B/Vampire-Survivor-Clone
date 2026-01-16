using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Components
    private Rigidbody rb;

    // Movement
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 4500;
    [SerializeField] private float sprintSpeed = 6000;
    [SerializeField] private float maxSpeed = 20;
    [SerializeField] private float sprintMaxSpeed = 25;
    [SerializeField] private float crouchMaxSpeed = 10;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private float counterMovement = 0.175f;

    // Jumping & Gravity
    [Header("Jump/Gravity Settings")]
    //Jumping
    private bool readyToJump = true;
    public float jumpCooldown = 0.25f;
    public float jumpForce = 550f;
    [SerializeField] private float gravityForce = 20f;

    // Crouch
    [Header("Crouch Settings")]
    [SerializeField] private Vector3 crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 playerScale;
    private bool crouching;
    private float currentMoveSpeed;
    private float currentMaxSpeed;

    // Camera Look
    [Header("Camera Settings")]
    [SerializeField] private Transform playerCam;
    [SerializeField] private Transform orientation;
    [SerializeField] private float sensitivity = 50f;
    private float xRotation;
    private float desiredX;

    // Input
    private float x, y;
    private bool jumpInput, crouchInput, sprintInput;

    // Sprint & Stamina
    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenRate = 15f;
    [SerializeField] private float staminaDrainRate = 20f;
    [SerializeField] private float staminaRegenDelay = 2f;
    private float currentStamina;
    private float timeSinceLastSprint;
    private bool canSprint = true;
    private bool sprinting;

    // Ground Check
    private bool grounded;
    private float groundCheckDistance = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerScale = transform.localScale;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        GroundCheck();
        GetInput();
        Look();
        HandleStamina();
    }

    void FixedUpdate()
    {
        Movement();
        HandleGravity();
    }
    void Start()
    {
        //SoundManager.EmitSound(transform.position, 20f, 30f);
    }

    private void GroundCheck()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, whatIsGround);
    }

    private void GetInput()
    {
        x = Input.GetAxisRaw("Horizontal");
        y = Input.GetAxisRaw("Vertical");
        jumpInput = Input.GetKey(KeyCode.Space);
        crouchInput = Input.GetKeyDown(KeyCode.C);
        sprintInput = Input.GetKey(KeyCode.LeftShift);
        

        if (crouchInput)
        {
            ToggleCrouch();
        }

        if (jumpInput)
        {
            Debug.Log(grounded);
            Jump();
        }
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.fixedDeltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.fixedDeltaTime;

        Vector3 rot = playerCam.transform.localRotation.eulerAngles;
        desiredX = rot.y + mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCam.transform.localRotation = Quaternion.Euler(xRotation, desiredX, 0);
        orientation.transform.localRotation = Quaternion.Euler(0, desiredX, 0);
    }

    private void ToggleCrouch()
    {
        crouching = !crouching;

        if (crouching)
        {
            transform.localScale = crouchScale;
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y - 0.5f,
                transform.position.z
            );
        }
        else
        {
            if (!Physics.Raycast(transform.position, Vector3.up, playerScale.y - crouchScale.y + 0.1f))
            {
                transform.localScale = playerScale;
                transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y + 0.5f,
                    transform.position.z
                );
            }
            else
            {
                crouching = true;
            }
        }
    }

    private void Jump() {
        if (grounded && readyToJump)
        {
            readyToJump = false;
            Debug.Log("Jump");
            //Add jump forces
            rb.AddForce(Vector3.up * jumpForce * 1.5f, ForceMode.Impulse);
            rb.AddForce(rb.velocity.normalized * jumpForce * 1f, ForceMode.Impulse);

            //If jumping while falling, reset y velocity.
            /*
            Vector3 vel = rb.velocity;
            if (rb.velocity.y < 0.5f)
                rb.velocity = new Vector3(vel.x, 0, vel.z);
            else if (rb.velocity.y > 0)
                rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);
            */
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    private void ResetJump() {
        readyToJump = true;
    }

    private void HandleGravity()
    {
        if (!grounded)
        {
            rb.velocity -= new Vector3(0, gravityForce * Time.fixedDeltaTime, 0);
        }
    }

    private void HandleStamina()
    {
        sprinting = sprintInput && canSprint && !crouching && y > 0 && currentStamina > 0;

        if (sprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            timeSinceLastSprint = 0f;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false;
            }
        }
        else
        {
            timeSinceLastSprint += Time.deltaTime;

            if (timeSinceLastSprint >= staminaRegenDelay)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);

                if (currentStamina >= maxStamina * 0.2f)
                {
                    canSprint = true;
                }
            }
        }
    }

    private void Movement()
    {
        if (crouching)
        {
            currentMoveSpeed = moveSpeed;
            currentMaxSpeed = crouchMaxSpeed;
        }
        else if (sprinting)
        {
            currentMoveSpeed = sprintSpeed;
            currentMaxSpeed = sprintMaxSpeed;
        }
        else
        {
            currentMoveSpeed = moveSpeed;
            currentMaxSpeed = maxSpeed;
        }

        Vector3 moveDirection = orientation.forward * y + orientation.right * x;
        rb.AddForce(moveDirection.normalized * currentMoveSpeed * Time.deltaTime);

        if (rb.velocity.magnitude > currentMaxSpeed)
        {
            rb.velocity = rb.velocity.normalized * currentMaxSpeed;
        }

        // Counter movement for better control
        /*
        if (Mathf.Abs(x) < 0.01f || Mathf.Abs(y) < 0.01f)
        {
            rb.velocity = new Vector3(
                rb.velocity.x * (1 - counterMovement),
                rb.velocity.y,
                rb.velocity.z * (1 - counterMovement)
            );
        }
        */
    }
    
}