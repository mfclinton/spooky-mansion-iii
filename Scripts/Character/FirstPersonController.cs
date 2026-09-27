using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FMODUnity;
using Random = UnityEngine.Random;
using UnityEngine.SceneManagement;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove {get; set;} = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => canJump && Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool ShouldCrouch => canCrouch && Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;
    private bool ShouldClimbLadder => canClimbLadder && isClimbingLadder;
    private bool ShouldPlayFootsteps => useFootsteps && characterController.isGrounded && currentInput != Vector2.zero;
    private float InjuredT => 1 - currentHealth / maxHealth;

    [Header("Functional Options")]
    [SerializeField] public bool canSprint = true;
    [SerializeField] public bool canJump = true;
    [SerializeField] public bool canCrouch = true;
    [SerializeField] public bool canUseHeadbob = true;
    [SerializeField] public bool willSlideOnSlopes = true;
    [SerializeField] public bool canZoom = true;
    [SerializeField] public bool useFootsteps = true;
    [SerializeField] public bool canInteract = true;
    [SerializeField] public bool useStamina = true;
    [SerializeField] public bool canGenerateNoise = true;
    [SerializeField] public bool canClimbLadder = true;
    [SerializeField] public bool canTakeDamage = true;
    [SerializeField] public bool canRegenerateHealth = false;
    [SerializeField] public bool canRegenerateStamina = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;
    public KeyCode InteractKey => interactKey;
    [SerializeField] private KeyCode zoomKey = KeyCode.Mouse1;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 3.0f;
    [SerializeField] private float sprintSpeed = 6.0f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float slopeSpeed = 8f;

    [Header("Aiming Parameters")]
    [SerializeField] private float aimedSpeedMultiplier = 0.75f;

    [Header("Look Parameters")]
    [SerializeField, Range(1,10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1,10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1,180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1,180)] private float lowerLookLimit = 80.0f;

    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100f;
    public float MaxHealth => maxHealth;
    [SerializeField] private float timeBeforeRegenStarts = 3f;
    [SerializeField] private float healthValueIncrement = 1f;
    [SerializeField] private float healthTimeIncrement = 0.1f;
    public float currentHealth {get; private set;}
    private Coroutine regeneratingHealthCoroutine;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100f;
    public float MaxStamina => maxStamina;
    [SerializeField] private float staminaUseMultiplier = 10f;
    [SerializeField] private float jumpStaminaUse = 25f;
    [SerializeField] private float timeBeforeStaminaRegenStarts = 5f;
    [SerializeField] private float staminaValueIncrement = 2f;
    [SerializeField] private float staminaTimeIncrement = 0.1f;
    public float currentStamina {get; private set;}
    private Coroutine regeneratingStaminaCoroutine;
    public static Action<float> OnStaminaChange;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 16.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0, 0, 0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 7f;
    [SerializeField] private float walkBobAmount = 0.02f;
    [SerializeField] private float sprintBobSpeed = 12f;
    [SerializeField] private float sprintBobAmount = 0.05f;
    [SerializeField] private float crouchBobSpeed = 4f;
    [SerializeField] private float crouchBobAmount = 0.01f;
    [SerializeField] private float maxInjuredBobSpeedMultiplier = 2f;
    [SerializeField] private float maxInjuredBobAmountMultiplier = 2f;
    private float defaultYPos = 0;
    private float timer = 0;

    [Header("Zoom Parameters")]
    [SerializeField] private float timeToZoom = 0.3f;
    [SerializeField] private float zoomFOV = 30f;
    private float defaultFOV;
    private Coroutine zoomCoroutine;
    private bool isZoomedIn = false;

    [Header("Footstep Parameters")]
    [SerializeField] private StudioEventEmitter footstepEmitter;
    [SerializeField] private StudioEventEmitter jumpEmitter;

    [Header("Noise Parameters")]
    [SerializeField] public float baseNoiseRadius = 2f;
    [SerializeField, Range(0f, 1f)] public float crouchNoiseRadiusMultiplier = 0.5f;
    [SerializeField, Range(1f,8f)] public float sprintNoiseRadiusMultiplier = 1.5f;
    [SerializeField, Range(1f,8f)] public float shootNoiseRadiusMultiplier = 3f;

    [Header("Ladder Parameters")]
    [SerializeField] public float ladderClimbSpeed = 100f;
    [SerializeField] public float ladderFallSpeed = 100f;
    public bool isClimbingLadder {get; set;} = false;

    [Header("Ambience")]
    [SerializeField] public StudioEventEmitter ambienceEmitter;


    // SLIDING PARAMETERS

    private Vector3 hitPointNormal;
    private bool isSliding
    {
        get
        {
            if (willSlideOnSlopes && characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
                return false;
        }
    }

    [Header("Interaction")]
    [SerializeField] private Vector3 interactionRayPoint = new Vector3(0.5f, 0.5f, 0);
    [SerializeField] private float interactionRayDistance = 2f;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentInteractable = default;

    // Dying Params
    bool isDying = false;

    private Camera playerCamera;
    private CharacterController characterController;
    public Gun gun {get; private set;}

    private Vector3 moveDirection;
    private Vector2 currentInput;
    
    private float rotationX = 0;

    // Inventory
    private HashSet<string> keyInventory;

    public static FirstPersonController Instance {get; private set;}

    private void Awake() {
        Instance = this;
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        gun = GetComponentInChildren<Gun>();
        playerCamera.GetComponent<Animator>().enabled = false;
        keyInventory = new HashSet<string>();

        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Gun.OnFire += (RaycastHit hit) => PropagateNoise(shootNoiseRadiusMultiplier * baseNoiseRadius);
    }

    // Subscribe to sceneUnloaded when enabled
    private void OnEnable() {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    // Unsubscribe when disabled
    private void OnDisable() {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    private void OnSceneUnloaded(Scene current) {
        OnDamage = null;
        OnHeal = null;
        OnStaminaChange = null;
    }

    private void Update() {
        if (CanMove) {
            HandleMovementInput();
            HandleMouseLook();
            HandleJump();
            HandleCrouch();
            HandleHeadbob();
            HandleZoom();
            HandleFootsteps();
            HandleInteraction();
            HandleStamina();
        }
    }

    private void FixedUpdate() {
        if (CanMove) {
            ApplyFinalMovements();
        }
    }

    private void HandleMovementInput() {
        float speed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
        if (gun.IsAimedIn)
            speed *= aimedSpeedMultiplier;

        currentInput = new Vector2(speed * Input.GetAxis("Vertical"), speed * Input.GetAxis("Horizontal"));
        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    private void HandleMouseLook() {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }

    private void HandleJump() {
        if (ShouldJump && jumpStaminaUse <= currentStamina) {
            moveDirection.y = jumpForce;
            currentStamina -= jumpStaminaUse;
            OnStaminaChange?.Invoke(currentStamina);
            jumpEmitter?.Play();
            jumpEmitter.SetParameter("Airborne", 1);

            PropagateNoise(baseNoiseRadius * sprintNoiseRadiusMultiplier);
        }

       
    }

    private void HandleCrouch() {
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private void HandleHeadbob() {
        if (!canUseHeadbob || !characterController.isGrounded)
            return;
        
        if (moveDirection.magnitude > 0.1f)
        {
            float bobSpeed = isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed;
            float bobAmount = isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount;

            bobSpeed *= Mathf.Lerp(1, maxInjuredBobSpeedMultiplier, InjuredT);
            bobAmount *= Mathf.Lerp(1, maxInjuredBobAmountMultiplier, InjuredT);

            timer += Time.deltaTime * bobSpeed;
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * bobAmount,
                playerCamera.transform.localPosition.z
            );
        }
    }

    private void HandleStamina() {
        if (!useStamina)
            return;

        if (IsSprinting && currentInput != Vector2.zero) {
            
            if (regeneratingStaminaCoroutine != null)
            {
                StopCoroutine(regeneratingStaminaCoroutine);
                regeneratingStaminaCoroutine = null;
            }

            currentStamina -= staminaUseMultiplier * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                canSprint = false;
            }

            OnStaminaChange?.Invoke(currentStamina);
        }

        if(!IsSprinting && currentStamina < maxStamina && regeneratingStaminaCoroutine == null)
            regeneratingStaminaCoroutine = StartCoroutine(RegenerateStamina());
    }

    private void HandleZoom() {
        if (!canZoom)
            return;

        if (Input.GetKeyDown(zoomKey))
        {
            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);

            isZoomedIn = !isZoomedIn;
            zoomCoroutine = StartCoroutine(ToggleZoom(isZoomedIn));
        }
    }

    private void HandleInteraction() {
        if (!canInteract)
            return;

        HandleInteractionCheck();
        HandleInteractionInput();
    }

    private void HandleInteractionCheck() {
        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionRayDistance)) {
            // Check if the hit layer is InteractableSolid or InteractableTrigger
            bool isInteractable = hit.collider.gameObject.layer == LayerMask.NameToLayer("InteractableSolid") || hit.collider.gameObject.layer == LayerMask.NameToLayer("InteractableTrigger");
            if(isInteractable && (currentInteractable == null || hit.collider.gameObject != currentInteractable.gameObject)) {
                hit.collider.TryGetComponent<Interactable>(out currentInteractable);
                
                if(currentInteractable != null)
                    currentInteractable.OnFocus();
            }
        }
        else if(currentInteractable != null) {
            currentInteractable.OnLoseFocus();
            currentInteractable = null;
        }
    }

    private void HandleInteractionInput() {
        if(Input.GetKeyDown(interactKey) && currentInteractable != null)
            currentInteractable.OnInteract();
    }

    private void ApplyFinalMovements() {
        // Process gravity
        if(!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;
        else if (characterController.velocity.y < -1 && characterController.isGrounded)
            moveDirection.y = 0;
        
        if (isSliding)
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;

        HandleLadder();

        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void HandleLadder() {
        if (!ShouldClimbLadder)
            return;
        
        if (currentInput.magnitude > 0.1f)
            moveDirection.y = ladderClimbSpeed * Time.deltaTime;
        else
            moveDirection.y = - ladderFallSpeed * Time.deltaTime;
    }

    private IEnumerator CrouchStand() {
        if(isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while (timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;

        duringCrouchAnimation = false;
    }

    private IEnumerator ToggleZoom(bool isEnter) {
        float targetFOV = isEnter ? zoomFOV : defaultFOV;
        float startFOV = playerCamera.fieldOfView;
        float timeElapsed = 0;

        while (timeElapsed < timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, timeElapsed / timeToZoom);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.fieldOfView = targetFOV;
        zoomCoroutine = null;
    }

    private void HandleFootsteps() {
        if (!ShouldPlayFootsteps)
        {
            if(footstepEmitter.IsPlaying())
                footstepEmitter.Stop();
            
            return;
        }


        // Sets the surface parameter for the footstep event
        // Ignores player layer
        LayerMask layerMask = ~(1 << LayerMask.NameToLayer("Player"));
        if(Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3, layerMask)) {
            switch (hit.collider.tag) {
                case "Footsteps/Stone":
                    footstepEmitter.SetParameter("Surface", 1);
                    break;
                case "Footsteps/Wood":
                    footstepEmitter.SetParameter("Surface", 2);
                    break;
                case "Footsteps/Stairs":
                    footstepEmitter.SetParameter("Surface", 3);
                    break;
                default:
                    footstepEmitter.SetParameter("Surface", 1);
                    break;
            }

            print(hit.collider.tag);
        }

        // Sets the speed parameter for the footstep event
        int currentSpeedIndex = isCrouching ? 0 : IsSprinting ? 2 : 1;
        footstepEmitter.SetParameter("MoveSpeed", currentSpeedIndex);

        // Alerts agents of noise
        ScanForNoiseListeners();

        if(!footstepEmitter.IsPlaying())
            footstepEmitter?.Play();
    }

    public void ApplyDamage(float damage, GameObject damageSource = null) {
        if (!canTakeDamage)
            return;

        currentHealth -= damage;
        OnDamage?.Invoke(currentHealth);

        if (currentHealth <= 0)
            KillPlayer(damageSource);
        else if (regeneratingHealthCoroutine != null)
            StopCoroutine(regeneratingHealthCoroutine);

        regeneratingHealthCoroutine = StartCoroutine(RegenerateHealth());
    }

    private void KillPlayer(GameObject deathSource = null) {
        if(isDying)
            return;
        
        currentHealth = 0;

        if (regeneratingHealthCoroutine != null)
            StopCoroutine(regeneratingHealthCoroutine);
        
        isDying = true;
        CanMove = false;
        gun.OnDeath();
        AnimateDeath();
        LevelManager.Instance.RestartLevel(useFade: true);
        // GAME OVER
    }

    private void AnimateDeath() {
        // assumes CanMove is false
        Animator camAnimator = playerCamera.GetComponent<Animator>();
        camAnimator.enabled = true;
        playerCamera.GetComponent<Animator>().SetTrigger("Death1");
    }

    private IEnumerator RegenerateHealth() {
        yield return new WaitForSeconds(timeBeforeRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while (canRegenerateHealth && currentHealth < maxHealth)
        {
            currentHealth += healthValueIncrement;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;

            OnHeal?.Invoke(currentHealth);
            yield return timeToWait;
        }

        regeneratingHealthCoroutine = null;
    }

    private IEnumerator RegenerateStamina() {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while (canRegenerateStamina && currentStamina < maxStamina)
        {
            currentStamina += staminaValueIncrement;

            if(0 < currentStamina)
                canSprint = true;

            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
            
            OnStaminaChange?.Invoke(currentStamina);

            yield return timeToWait;
        }

        regeneratingStaminaCoroutine = null;
    }

    private void ScanForNoiseListeners() {
        if(!canGenerateNoise)
            return;

        float noiseRadius = isCrouching ? baseNoiseRadius * crouchNoiseRadiusMultiplier : IsSprinting ? baseNoiseRadius * sprintNoiseRadiusMultiplier : baseNoiseRadius;
        PropagateNoise(noiseRadius);
    }

    private void PropagateNoise(float noiseRadius) {
        // Layer mask for enemy and noise listeners
        int layerMask = (1 << LayerMask.NameToLayer("Enemy")) | (1 << LayerMask.NameToLayer("NoiseListener"));

        Collider[] colliders = Physics.OverlapSphere(transform.position, noiseRadius, layerMask);

        foreach (Collider collider in colliders)
        {
            NoiseListener listener = collider.GetComponentInParent<NoiseListener>();
            if (listener != null)
                listener.OnNoiseHeard(transform.position);
        }
    }

    public void PickupKey(string keyName) {
        keyInventory.Add(keyName);
    }

    public bool HasKey(string keyName) {
        return keyInventory.Contains(keyName);
    }

    public bool UseKey(string keyName) {
        bool hasKey = HasKey(keyName);
        if(hasKey)
            keyInventory.Remove(keyName);
        return hasKey;
    }
}
