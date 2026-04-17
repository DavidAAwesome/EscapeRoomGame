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
    [SerializeField] private float standCenterY;
    
    [Header("Footstep Noise")]
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField] private AudioClip[] crouchClips;
    [SerializeField] private float walkStepInterval   = 0.5f;
    [SerializeField] private float crouchStepInterval = 0.9f;
    [SerializeField] private float walkNoiseRadius   = 12f;
    [SerializeField] private float crouchNoiseRadius = 4f;
    
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
    
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        interactionSystem = GetComponent<InteractionSystem>();
        audio = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();
        
        standHeight = characterController.height;
        crouchHeight = standHeight * 0.5f;
        standCenterY = characterController.center.y;
        targetHeight = standHeight;

        LockCursor();
    }

    private void Update()
    {
        if (isFrozen) return;
        HandleLook();
        HandleCrouch();
        HandleMove();
        HandleFootsteps();
    }
    
    public void OnInteract(InputValue value)
    {
        if (isFrozen) return;
        Debug.Log("E pressed!");
        if (interactionSystem != null)
            interactionSystem.Interact();
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
        Debug.Log("C pressed!");
        if (!value.isPressed) return;

        isCrouching = !isCrouching;
        targetHeight = isCrouching ? crouchHeight : standHeight;
    }
    
    private void HandleCrouch()
    {
        if (Mathf.Abs(characterController.height - targetHeight) > 0.01f)
        {
            float newHeight = Mathf.Lerp(characterController.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);

            if (Mathf.Abs(newHeight - targetHeight) < 0.01f)
                newHeight = targetHeight;

            characterController.height = newHeight;

            // keep the original center setup
            Vector3 center = characterController.center;
            center.y = standCenterY;
            characterController.center = center;
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
    
    private void HandleFootsteps()
    {
        if (!isMoving || !characterController.isGrounded) return;
 
        float interval = isCrouching ? crouchStepInterval : walkStepInterval;
        stepTimer += Time.deltaTime;
 
        if (stepTimer >= interval)
        {
            stepTimer = 0f;
            PlayFootstepAudio();
            EmitNoise();
        }
    }
    
    private void PlayFootstepAudio()
    {
        AudioClip[] clips = isCrouching ? crouchClips : walkClips;
        if (clips == null || clips.Length == 0) return;
        audio.PlayOneShot(clips[UnityEngine.Random.Range(0, clips.Length)]);
    }
 
    private void EmitNoise()
    {
        float radius = isCrouching ? crouchNoiseRadius : walkNoiseRadius;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (var hit in hits)
        {
            GrannyAI granny = hit.GetComponent<GrannyAI>();
            if (granny != null)
                granny.AlertToSound(transform.position);
        }
    }
    
    public void FreezePlayer()
    {
        isFrozen = true;
        if (characterController != null)
            characterController.enabled = false;
    }
    
    public void UnfreezePlayer()
    {
        isFrozen = false;
        if (characterController != null)
            characterController.enabled = true;
    }
    
    public void GameStarting()
    {
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        UnfreezePlayer();
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("WinTrigger"))
            GameManager.Instance.OnPlayerEscaped();
    }
}
