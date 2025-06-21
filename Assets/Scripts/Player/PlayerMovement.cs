using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using UnityEngine;
using Photon.Pun;



public enum MovementState
{
    walking,
    sprinting,
    crouching,
    sliding,
    air
}

public class PlayerMovement : MonoBehaviour
{
    private PhotonView photonView;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed;

    public float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    [HideInInspector] public float speedIncreaseMultiplier;
    [HideInInspector] public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("ScoreBoard Settings")]
    [SerializeField] private GameObject scoreboardUIPrefab;
    private GameObject scoreboardInstance;
    private ScoreboardUI scoreboardUI;

    public Transform orientation;
    public Transform armaTransform;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public bool sliding;

    private bool inputEnabled = true;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;
        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        if (!inputEnabled) return;

        if (!photonView.IsMine) return;



        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        rb.drag = grounded ? groundDrag : 0;
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;
        MovePlayer();
    }

    public void SetInput(bool isEnabled)
    {
        inputEnabled = isEnabled;
    }
  
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
            armaTransform.localScale = new Vector3(1f, 2f, 1f);
        }

        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            armaTransform.localScale = new Vector3(1f, 1f, 1f);
        }

        // Scoreboard
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ShowScoreboard(true);
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            ShowScoreboard(false);
        }
    }

    private void StateHandler()
    {
        if (sliding)
        {
            state = MovementState.sliding;
            desiredMoveSpeed = (OnSlope() && rb.velocity.y < 0.1f) ? slideSpeed : sprintSpeed;
        }
        else if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }

        float acceleration = (grounded ? 10f : 4f);
        moveSpeed = Mathf.MoveTowards(moveSpeed, desiredMoveSpeed, Time.deltaTime * acceleration);

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (GetComponent<LadderClimbing>()?.isClimbing == true)
            return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    public bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    private void ShowScoreboard(bool show)
    {
        if (show)
        {
            if (scoreboardInstance == null)
            {
                scoreboardInstance = Instantiate(scoreboardUIPrefab);
                scoreboardUI = scoreboardInstance.GetComponent<ScoreboardUI>();
            }

            scoreboardUI.UpdateScoreboard(GameManager.Instance.PlayerStats);
            scoreboardInstance.SetActive(true);
        }
        else if (scoreboardInstance != null)
        {
            scoreboardInstance.SetActive(false);
        }
    }

    public void DesactiveScoreBoard()
    {
        if (scoreboardInstance != null)
            scoreboardInstance.SetActive(false);
    }
}

