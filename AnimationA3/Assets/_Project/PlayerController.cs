using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerNew : MonoBehaviour
{
    private static readonly int Speed = Animator.StringToHash("Speed");

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference sprintAction;

    [Header("Movement")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float accelerationTime;
    [SerializeField] private float decelerationTime;
    [SerializeField] private float gravity;
    
    [Header("Animation")]
    [SerializeField] private Animator playerAnimator;
    

    private CharacterController _characterController;

    private Vector2 _currentInput;
    private Vector2 _inputSmoothingVelocity;
    private float _verticalVelocity;

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        sprintAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        sprintAction.action.Disable();
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 targetInput = moveAction.action.ReadValue<Vector2>();

        if (targetInput.sqrMagnitude > 1f) targetInput.Normalize();

        float smoothingTime = targetInput.sqrMagnitude > 0f ? accelerationTime : decelerationTime;
        _currentInput = Vector2.SmoothDamp(_currentInput, targetInput, ref _inputSmoothingVelocity, smoothingTime);
        bool isSprinting = sprintAction.action.IsPressed();
        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;
        
        float animationSpeed = _currentInput.magnitude * currentSpeed/ sprintSpeed;
        playerAnimator.SetFloat(Speed, animationSpeed);

        Vector3 horizontalMovement =
            transform.right * _currentInput.x +
            transform.forward * _currentInput.y;

        if (_characterController.isGrounded && _verticalVelocity < 0f)
        {
            _verticalVelocity = -2f;
        }

        _verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMovement =
            horizontalMovement * currentSpeed +
            Vector3.up * _verticalVelocity;

        _characterController.Move(finalMovement * Time.deltaTime);
    }
}