using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;

    [Header("Turning")]
    [SerializeField] private Transform characterVisual;
    [SerializeField] private float turn90Duration = 0.35f;
    [SerializeField] private float turn180Duration = 0.55f;
    [SerializeField] private float backwardTimeBeforeTurn = 0.35f;
    [SerializeField] private bool turnAroundOnGameplayStart = true;

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
    private bool _isTurning;
    private bool _controlsEnabled;
    private bool _startupTurnCompleted;

    private float _backwardTimer;

    private Quaternion _startingVisualLocalRotation;
    private Coroutine _turnCoroutine;

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

    private static readonly int Turn90 =
        Animator.StringToHash("Turn90");

    private static readonly int Turn180 =
        Animator.StringToHash("Turn180");
    
    private static readonly int IsJumping =
        Animator.StringToHash("isJumping");

    private static readonly int IsGroundedParameter =
        Animator.StringToHash("isGrounded");

    private static readonly int IsFalling =
        Animator.StringToHash("isFalling");

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        if (characterVisual == null && playerAnimator != null)
        {
            characterVisual = playerAnimator.transform;
        }

        _startingVisualLocalRotation =
            characterVisual.localRotation;

        _rigidbody.useGravity = true;

        _rigidbody.interpolation =
            RigidbodyInterpolation.Interpolate;

        _rigidbody.collisionDetectionMode =
            CollisionDetectionMode.Continuous;

        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnEnable()
    {
        moveInputAction.action.Enable();
        sprintInputAction.action.Enable();
        jumpInputAction.action.Enable();

        _moveInput = Vector2.zero;
        _backwardTimer = 0f;

        if (turnAroundOnGameplayStart &&
            !_startupTurnCompleted)
        {
            _controlsEnabled = false;

            characterVisual.localRotation =
                _startingVisualLocalRotation;

            _turnCoroutine =
                StartCoroutine(BeginGameplayTurn());
        }
        else
        {
            _controlsEnabled = true;
        }
    }

    private void OnDisable()
    {
        moveInputAction.action.Disable();
        sprintInputAction.action.Disable();
        jumpInputAction.action.Disable();

        if (_turnCoroutine != null)
        {
            StopCoroutine(_turnCoroutine);
            _turnCoroutine = null;
        }

        _controlsEnabled = false;
        _isTurning = false;
    }

    private void Update()
    {
        _moveInput = _controlsEnabled ? moveInputAction.action.ReadValue<Vector2>() : Vector2.zero;

        _isSprinting =
            _controlsEnabled &&
            !_isTurning &&
            _moveInput.sqrMagnitude > 0.01f &&
            sprintInputAction.action.IsPressed();

        if (_controlsEnabled &&
            !_isTurning &&
            jumpInputAction.action.WasPressedThisFrame())
        {
            _jumpRequested = true;
        }

        if (_controlsEnabled && !_isTurning)
        {
            Vector3 movementDirection =
                GetWorldMovementDirection();

            HandleFacing(movementDirection);
        }

        UpdateLocomotionAnimation();
        UpdateJumpAnimation();
    }
    

    private void FixedUpdate()
    {
        if (!_controlsEnabled || _isTurning)
        {
            StopHorizontalMovement();
            return;
        }

        HandleMovement();
        HandleJump();
    }

    private void HandleMovement()
    {
        Vector3 movementDirection =
            GetWorldMovementDirection();

        float currentSpeed =
            _isSprinting
                ? runSpeed
                : walkSpeed;

        Vector3 velocity =
            movementDirection * currentSpeed;

        velocity.y =
            _rigidbody.linearVelocity.y;

        _rigidbody.linearVelocity =
            velocity;
    }

    private Vector3 GetWorldMovementDirection()
    {
        Vector3 cameraForward =
            cameraTransform.forward;

        Vector3 cameraRight =
            cameraTransform.right;

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

        return movementDirection;
    }

    private void HandleFacing(
        Vector3 movementDirection)
    {
        if (movementDirection.sqrMagnitude < 0.01f)
        {
            _backwardTimer = 0f;
            return;
        }
        
        if (!IsGrounded())
        {
            _backwardTimer = 0f;
            return;
        }

        // Diagonal input uses the diagonal animations.
        // It does NOT force Michelle to turn.
        if (!IsCardinalInput())
        {
            _backwardTimer = 0f;
            return;
        }

        Vector3 facingDirection =
            GetFacingDirection();

        float angle =
            Vector3.Angle(
                facingDirection,
                movementDirection
            );

        // Directly opposite:
        // walk backwards briefly, then turn around.
        if (angle > 135f)
        {
            _backwardTimer += Time.deltaTime;

            if (_backwardTimer >=
                backwardTimeBeforeTurn)
            {
                StartTurn(movementDirection);
            }

            return;
        }

        _backwardTimer = 0f;

        // Roughly perpendicular:
        // W/S when currently facing left/right,
        // or A/D when facing up/down.
        if (angle > 45f)
        {
            StartTurn(movementDirection);
        }
    }

    private bool IsCardinalInput()
    {
        bool horizontal =
            Mathf.Abs(_moveInput.x) > 0.5f &&
            Mathf.Abs(_moveInput.y) < 0.1f;

        bool vertical =
            Mathf.Abs(_moveInput.y) > 0.5f &&
            Mathf.Abs(_moveInput.x) < 0.1f;

        return horizontal || vertical;
    }

    private Vector3 GetFacingDirection()
    {
        Vector3 forward =
            characterVisual.forward;

        forward.y = 0f;

        return forward.normalized;
    }

    private void StartTurn(
        Vector3 targetDirection)
    {
        if (_isTurning)
        {
            return;
        }

        _backwardTimer = 0f;

        _turnCoroutine =
            StartCoroutine(
                TurnToDirection(targetDirection)
            );
    }

    private IEnumerator BeginGameplayTurn()
    {
        // Gameplay Michelle starts facing LEFT.
        // Turn 180 degrees before controls unlock.

        Vector3 startingDirection =
            GetFacingDirection();

        Vector3 oppositeDirection =
            -startingDirection;

        yield return
            TurnToDirection(oppositeDirection);

        _startupTurnCompleted = true;
        _controlsEnabled = true;
        _turnCoroutine = null;
    }

    private IEnumerator TurnToDirection(
        Vector3 targetDirection)
    {
        _isTurning = true;

        targetDirection.y = 0f;
        targetDirection.Normalize();

        Vector3 currentDirection =
            GetFacingDirection();

        float angle =
            Vector3.SignedAngle(
                currentDirection,
                targetDirection,
                Vector3.up
            );

        Quaternion startRotation =
            characterVisual.rotation;

        Quaternion targetRotation =
            Quaternion.AngleAxis(
                angle,
                Vector3.up
            ) * startRotation;

        float absoluteAngle = Mathf.Abs(angle);

        bool is180Turn =
            absoluteAngle > 135f;

        float currentTurnDuration =
            is180Turn
                ? turn180Duration
                : turn90Duration;

        playerAnimator.ResetTrigger(Turn90);
        playerAnimator.ResetTrigger(Turn180);

        playerAnimator.SetTrigger(
            is180Turn
                ? Turn180
                : Turn90
        );

        float elapsed = 0f;

        while (elapsed < currentTurnDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed / currentTurnDuration
                );

            characterVisual.rotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    t
                );

            yield return null;
        }

        characterVisual.rotation =
            targetRotation;

        _isTurning = false;
        _turnCoroutine = null;
    }

    private void UpdateLocomotionAnimation()
    {
        
        if (_isTurning)
        {
            playerAnimator.SetFloat(
                InputMagnitude,
                0f,
                animationDampTime,
                Time.deltaTime
            );

            playerAnimator.SetBool(
                IsIdle,
                false
            );

            playerAnimator.SetBool(
                IsWalking,
                false
            );

            playerAnimator.SetBool(
                IsRunning,
                false
            );

            return;
        }

        Vector3 movementDirection =
            GetWorldMovementDirection();

        bool isMoving =
            movementDirection.sqrMagnitude >
            0.01f;

        float animationX = 0f;
        float animationY = 0f;

        if (isMoving)
        {
            // THIS is the important part:
            // convert WORLD movement into movement
            // relative to Michelle's current facing.
            Vector3 localMovement =
                characterVisual
                    .InverseTransformDirection(
                        movementDirection
                    );

            animationX = localMovement.x;
            animationY = localMovement.z;
        }

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
            animationX,
            animationDampTime,
            Time.deltaTime
        );

        playerAnimator.SetFloat(
            InputY,
            animationY,
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

    private void StopHorizontalMovement()
    {
        Vector3 velocity =
            _rigidbody.linearVelocity;

        velocity.x = 0f;
        velocity.z = 0f;

        _rigidbody.linearVelocity =
            velocity;
    }

    private void UpdateJumpAnimation()
    {
        
        bool grounded = IsGrounded();

        float verticalVelocity =
            _rigidbody.linearVelocity.y;

        bool jumping =
            !grounded &&
            verticalVelocity > 0.05f;

        bool falling =
            !grounded &&
            verticalVelocity < -0.05f;

        playerAnimator.SetBool(
            IsGroundedParameter,
            grounded
        );

        playerAnimator.SetBool(
            IsJumping,
            jumping
        );

        playerAnimator.SetBool(
            IsFalling,
            falling
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

        _rigidbody.linearVelocity =
            velocity;

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