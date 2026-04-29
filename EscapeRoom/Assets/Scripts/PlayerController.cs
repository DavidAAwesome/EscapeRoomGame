using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Controller Look")]
    [SerializeField] private float controllerLookSensitivity = 180f;
    [SerializeField] private float controllerLookSmoothing = 12f;

    [Header("Crouch")]
    [SerializeField] private float standHeight = 1.8f;
    [SerializeField] private float crouchHeight = 0.9f;
    [SerializeField] private float crouchTransitionSpeed = 8f;

    [Header("Footstep Noise")]
    [SerializeField] private AudioClip[] walkClips;
    [SerializeField] private AudioClip[] crouchClips;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float crouchStepInterval = 0.9f;
    [SerializeField] private float walkNoiseRadius = 12f;
    [SerializeField] private float crouchNoiseRadius = 4f;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    private CharacterController characterController;
    private InteractionSystem interactionSystem;
    private AudioSource audioSource;
    private PlayerInput playerInput;

    private Vector2 moveInput;
    private Vector2 lookInput;
    private Vector2 smoothedControllerLook;

    private float verticalVelocity;
    private float cameraPitch;
    private float stepTimer;
    private float targetHeight;
    private float standCenterY;
    private float scrollAccumulator;

    public bool isCrouching;
    public bool isMoving;

    private bool isFrozen;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        interactionSystem = GetComponent<InteractionSystem>();
        audioSource = GetComponent<AudioSource>();
        playerInput = GetComponent<PlayerInput>();

        standHeight = characterController.height;
        crouchHeight = standHeight * 0.5f;
        standCenterY = characterController.center.y;
        targetHeight = standHeight;

        LockCursor();
    }

    void Update()
    {
        if (isFrozen)
            return;

        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            return;

        HandleLook();
        HandleCrouch();
        HandleMove();
        HandleFootsteps();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnLook(InputValue value)
    {
        lookInput = value.Get<Vector2>();
    }

    public void OnInteract(InputValue value)
    {
        if (isFrozen) return;
        if (!value.isPressed) return;

        if (interactionSystem != null)
            interactionSystem.Interact();
    }

    public void OnCrouch(InputValue value)
    {
        if (isFrozen) return;
        if (!value.isPressed) return;

        isCrouching = !isCrouching;
        targetHeight = isCrouching ? crouchHeight : standHeight;
    }

    public void OnPause(InputValue value)
    {
        if (!value.isPressed) return;

        if (GameManager.Instance == null)
            return;

        if (GameManager.Instance.CurrentState == GameManager.State.Playing)
            GameManager.Instance.PauseGame();
        else if (GameManager.Instance.CurrentState == GameManager.State.Paused)
            GameManager.Instance.ResumeGame();
            
    }
    
    public void OnSelectSlot(InputValue value)
    {
        if (isFrozen) return;

        float rawValue = value.Get<float>();

        if (rawValue <= 0f)
            return;

        int slotIndex = Mathf.RoundToInt(rawValue) - 1;

        if (interactionSystem != null)
            interactionSystem.SelectSlot(slotIndex);
    }

    public void OnScrollSlot(InputValue value)
    {
        if (isFrozen) return;

        float scrollValue = value.Get<float>();

        // Controller bumpers / d-pad usually send clean -1 or +1 values
        if (Mathf.Abs(scrollValue) >= 1f)
        {
            int direction = scrollValue > 0 ? 1 : -1;

            if (interactionSystem != null)
                interactionSystem.CycleSlot(direction);

            return;
        }

        // Mouse wheel usually sends smaller values, so accumulate it
        scrollAccumulator += scrollValue;

        while (scrollAccumulator >= 1f)
        {
            if (interactionSystem != null)
                interactionSystem.CycleSlot(1);

            scrollAccumulator -= 1f;
        }

        while (scrollAccumulator <= -1f)
        {
            if (interactionSystem != null)
                interactionSystem.CycleSlot(-1);

            scrollAccumulator += 1f;
        }
    }

    private void HandleLook()
    {
        if (playerCamera == null)
            return;

        bool usingGamepad =
            playerInput != null &&
            playerInput.currentControlScheme != null &&
            playerInput.currentControlScheme.ToLower().Contains("gamepad");

        float yaw;
        float pitch;

        if (usingGamepad)
        {
            smoothedControllerLook = Vector2.Lerp(
                smoothedControllerLook,
                lookInput,
                Time.deltaTime * controllerLookSmoothing
            );

            yaw = smoothedControllerLook.x * controllerLookSensitivity * Time.deltaTime;
            pitch = -smoothedControllerLook.y * controllerLookSensitivity * Time.deltaTime;
        }
        else
        {
            yaw = lookInput.x * mouseSensitivity;
            pitch = -lookInput.y * mouseSensitivity;
        }

        cameraPitch = Mathf.Clamp(cameraPitch + pitch, -maxLookAngle, maxLookAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
        transform.Rotate(Vector3.up * yaw);
    }

    private void HandleCrouch()
    {
        if (Mathf.Abs(characterController.height - targetHeight) <= 0.01f)
            return;

        float newHeight = Mathf.Lerp(
            characterController.height,
            targetHeight,
            Time.deltaTime * crouchTransitionSpeed
        );

        if (Mathf.Abs(newHeight - targetHeight) < 0.01f)
            newHeight = targetHeight;

        characterController.height = newHeight;

        Vector3 center = characterController.center;
        center.y = standCenterY;
        characterController.center = center;
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
        if (!isMoving || !characterController.isGrounded)
            return;

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

        if (clips == null || clips.Length == 0 || audioSource == null)
            return;

        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
    }

    private void EmitNoise()
    {
        float radius = isCrouching ? crouchNoiseRadius : walkNoiseRadius;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (Collider hit in hits)
        {
            GrannyAI granny = hit.GetComponent<GrannyAI>();

            if (granny != null)
                granny.AlertToSound(transform.position);
        }
    }

    public void FreezePlayer()
    {
        isFrozen = true;
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;

        if (characterController != null)
            characterController.enabled = false;

        UnlockCursor();
    }

    public void UnfreezePlayer()
    {
        isFrozen = false;

        if (characterController != null)
            characterController.enabled = true;

        LockCursor();
    }

    public void GameStarting()
    {
        if (spawnPoint != null)
        {
            if (characterController != null)
                characterController.enabled = false;

            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;

            if (characterController != null)
                characterController.enabled = true;
        }

        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        smoothedControllerLook = Vector2.zero;
        verticalVelocity = 0f;
        stepTimer = 0f;

        isCrouching = false;
        targetHeight = standHeight;

        if (characterController != null)
        {
            characterController.height = standHeight;

            Vector3 center = characterController.center;
            center.y = standCenterY;
            characterController.center = center;
        }

        interactionSystem.ClearInventory();

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
        if (other.CompareTag("WinTrigger"))
            GameManager.Instance?.OnPlayerEscaped();
    }
}