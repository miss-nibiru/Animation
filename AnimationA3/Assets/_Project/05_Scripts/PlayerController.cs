using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 6f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Animator playerAnimator;

    [Header("Input")]
    [SerializeField] private InputActionReference moveInputAction;
    [SerializeField] private InputActionReference sprintInputAction;
    [SerializeField] private InputActionReference jumpInputAction;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayers = ~0;

    [Header("Animation")]
    [SerializeField] private float animationDampTime = 0.1f;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;

    private Vector2 _moveInput;
    private bool _jumpRequested;
    private bool _isSprinting;

    private static readonly int InputX =
        Animator.StringToHash("InputX");

    private static readonly int InputY =
        Animator.StringToHash("InputY");

    private static readonly int InputMagnitude =
        Animator.StringToHash("InputMagnitude");

    private static readonly int IsIdle =
        Animator.StringToHash("isIdle");

    private static readonly int IsWalking =
        Animator.StringToHash("isWalking");

    private static readonly int IsRunning =
        Animator.StringToHash("isRunning");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _rigidbody.useGravity = true;
        _rigidbody.interpolation =
            RigidbodyInterpolation.Interpolate;

        _rigidbody.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        _rigidbody.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnEnable()
    {
        moveInputAction.action.Enable();
        sprintInputAction.action.Enable();
        jumpInputAction.action.Enable();
    }

    private void OnDisable()
    {
        moveInputAction.action.Disable();
        sprintInputAction.action.Disable();
        jumpInputAction.action.Disable();
    }

    private void Update()
    {
        _moveInput =
            moveInputAction.action.ReadValue<Vector2>();

        _isSprinting =
            sprintInputAction.action.IsPressed() &&
            _moveInput.sqrMagnitude > 0.01f;

        if (jumpInputAction.action.WasPressedThisFrame())
        {
            _jumpRequested = true;
        }

        UpdateLocomotionAnimation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;

        cameraForward.y = 0f;
        cameraRight.y = 0f;

        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 movementDirection =
            cameraRight * _moveInput.x +
            cameraForward * _moveInput.y;

        if (movementDirection.sqrMagnitude > 1f)
        {
            movementDirection.Normalize();
        }

        float currentSpeed =
            _isSprinting ? runSpeed : walkSpeed;

        Vector3 velocity =
            movementDirection * currentSpeed;

        velocity.y = _rigidbody.linearVelocity.y;

        _rigidbody.linearVelocity = velocity;

        // IMPORTANT:
        // Do NOT rotate Michelle toward movement.
        //
        // W = forward animation
        // S = backwards animation
        // A/D = strafe animations.
    }

    private void UpdateLocomotionAnimation()
    {
        bool isMoving =
            _moveInput.sqrMagnitude > 0.01f;

        float targetMagnitude;

        if (!isMoving)
        {
            targetMagnitude = 0f;
        }
        else if (_isSprinting)
        {
            targetMagnitude = 1f;
        }
        else
        {
            targetMagnitude = 0.5f;
        }

        playerAnimator.SetFloat(
            InputX,
            _moveInput.x,
            animationDampTime,
            Time.deltaTime
        );

        playerAnimator.SetFloat(
            InputY,
            _moveInput.y,
            animationDampTime,
            Time.deltaTime
        );

        playerAnimator.SetFloat(
            InputMagnitude,
            targetMagnitude,
            animationDampTime,
            Time.deltaTime
        );

        playerAnimator.SetBool(
            IsIdle,
            !isMoving
        );

        playerAnimator.SetBool(
            IsWalking,
            isMoving && !_isSprinting
        );

        playerAnimator.SetBool(
            IsRunning,
            isMoving && _isSprinting
        );
    }

    private void HandleJump()
    {
        if (!_jumpRequested)
        {
            return;
        }

        _jumpRequested = false;

        if (!IsGrounded())
        {
            return;
        }

        Vector3 velocity =
            _rigidbody.linearVelocity;

        velocity.y = 0f;

        _rigidbody.linearVelocity = velocity;

        _rigidbody.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );
    }

    private bool IsGrounded()
    {
        Vector3 origin =
            _capsuleCollider.bounds.center;

        float distance =
            _capsuleCollider.bounds.extents.y +
            groundCheckDistance;

        return Physics.Raycast(
            origin,
            Vector3.down,
            distance,
            groundLayers,
            QueryTriggerInteraction.Ignore
        );
    }
}