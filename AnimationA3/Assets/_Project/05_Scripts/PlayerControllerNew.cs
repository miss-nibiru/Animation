using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerControllerNew : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float rotationSpeed = 12f;
    [SerializeField] private float jumpForce = 6f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform characterVisual;
    [SerializeField] private Animator playerAnimator;

    [Header("Input")]
    [SerializeField] private InputActionReference moveInputAction;
    [SerializeField] private InputActionReference jumpInputAction;

    [Header("Ground Detection")]
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private LayerMask groundLayers = ~0;

    private Rigidbody _rigidbody;
    private CapsuleCollider _capsuleCollider;

    private Vector2 _moveInput;
    private bool _jumpRequested;

    private static readonly int Walking =
        Animator.StringToHash("Walking");

    private static readonly int Running =
        Animator.StringToHash("Running");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        _rigidbody.useGravity = true;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        // Physics must never rotate the player collider.
        _rigidbody.constraints =
            RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        
    }

    private void OnEnable()
    {
        moveInputAction.action.Enable();
        jumpInputAction.action.Enable();
    }

    private void OnDisable()
    {
        moveInputAction.action.Disable();
        jumpInputAction.action.Disable();
    }

    private void Update()
    {
        _moveInput =
            moveInputAction.action.ReadValue<Vector2>();

        if (jumpInputAction.action.WasPressedThisFrame())
        {
            _jumpRequested = true;
        }

        UpdateAnimation();
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

        Vector3 velocity =
            movementDirection * movementSpeed;

        velocity.y = _rigidbody.linearVelocity.y;

        _rigidbody.linearVelocity = velocity;
        _rigidbody.angularVelocity = Vector3.zero;

        RotateVisual(movementDirection);
    }

    private void RotateVisual(Vector3 movementDirection)
    {
        if (movementDirection.sqrMagnitude < 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(
                movementDirection,
                Vector3.up
            );

        Quaternion smoothRotation =
            Quaternion.Slerp(
                _rigidbody.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

        _rigidbody.MoveRotation(smoothRotation);
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

        Vector3 velocity = _rigidbody.linearVelocity;
        velocity.y = 0f;
        _rigidbody.linearVelocity = velocity;

        _rigidbody.AddForce(
            Vector3.up * jumpForce,
            ForceMode.Impulse
        );
    }

    private bool IsGrounded()
    {
        Vector3 origin = _capsuleCollider.bounds.center;

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

    private void UpdateAnimation()
    {
        bool isWalking = _moveInput.sqrMagnitude > 0.01f;

        playerAnimator.SetBool(Walking, isWalking);
        playerAnimator.SetBool(Running, false);
    }
}