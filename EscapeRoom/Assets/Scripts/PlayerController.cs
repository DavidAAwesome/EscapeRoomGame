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
    
    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    
    private CharacterController characterController;
    private InteractionSystem interactionSystem;
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
    private bool isFrozen;


    public event Action InteractPressed;
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        interactionSystem = GetComponent<InteractionSystem>();
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
    
    public void OnInteract(InputValue value)
    {
        Debug.Log("E pressed!");
        if (interactionSystem != null)
            interactionSystem.Interact();
    }
    
    public void OnLook(InputValue value)
    {
        if (isFrozen) return;
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
        if (isFrozen) return;
        Debug.Log("C pressed!");
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
        if (isFrozen) return;
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
        isFrozen = true;
        // if using Rigidbody:
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
    
    public void UnfreezePlayer()
    {
        isFrozen = false;
    }
    
    public void ResetPlayer()
    {
        UnfreezePlayer();

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = spawnPoint.position;
            rb.rotation = spawnPoint.rotation;
        }
        else
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
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

    // private void OnCollisionEnter(Collision other)
    // {
    //     if (other.gameObject.CompareTag("Enemy"))
    //         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    // }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("WinTrigger"))
            GameManager.Instance.OnPlayerEscaped();
    }
}
