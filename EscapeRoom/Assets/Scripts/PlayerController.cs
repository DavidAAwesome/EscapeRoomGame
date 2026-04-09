using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float crouchSpeed = 1.8f;
    [SerializeField] private float gravity = -18f;
    
    [Header("Look")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 0.15f;
    [SerializeField] private float maxLookAngle = 80f;
    
    [Header("Crouch")]
    [SerializeField] private float standHeight = 1.8f;
    [SerializeField] private float crouchHeight = 0.9f;
    [SerializeField] private float crouchTransitionSpeed = 8f;
    
    private CharacterController characterController;
    private AudioSource audio;
    private PlayerInput playerInput;
    private Vector2 moveInput;
    private Vector2 lookInput;
    // private bool isGamepad;

    private float verticalVelocity;
    private float cameraPitch;
    private float stepTimer;
    private float targetHeight;

    public bool isCrouching;
    public bool isMoving;

    public event Action InteractPressed;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        audio = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();

        // targetHeight = standHeight;
        // characterController.height = standHeight;
        // characterController.center = new Vector3(0f, characterController.height / 2f, 0f);

        LockCursor();
    }

    private void Update()
    {
        HandleLook();
        // HandleCrouch();
        HandleMove();

    }
    
    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    private void HandleLook()
    {
        float yaw = lookInput.x * mouseSensitivity;
        float pitch = -lookInput.y * mouseSensitivity;

        cameraPitch = Mathf.Clamp(cameraPitch + pitch, -maxLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * yaw);
    }

    public void OnCrouch(InputValue value)
    {
        if (!value.isPressed) return;

        isCrouching = !isCrouching;
        targetHeight = isCrouching ? crouchHeight : standHeight;
    }
    
    private void HandleCrouch()
    {
        if (!Mathf.Approximately(characterController.height, targetHeight))
        {
            characterController.height = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            characterController.center = new Vector3(0f, characterController.height / 2f, 0f);
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
    
    private void HandleMove()
    {
        isMoving = moveInput.sqrMagnitude > 0.0025f;
        float speed = isCrouching ? crouchSpeed : walkSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move = Vector3.ClampMagnitude(move, 1f) * speed;

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        characterController.Move(move * Time.deltaTime);
    }
    
    public void FreezePlayer()
    {
        enabled = false;
        characterController.enabled = false;

        if (playerInput != null)
            playerInput.enabled = false;

        UnlockCursor();
    }
    
    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Enemy"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
