using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;

public class DiverMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Force applied when pressing a flipper button")]
    public float forwardForce = 5f;
    
    [Tooltip("Torque force applied for rotation")]
    public float torqueForce = 2f;
    
    [Tooltip("Maximum velocity the diver can move at")]
    public float maxVelocity = 10f;
    
    [Tooltip("Maximum angular velocity for rotation")]
    public float maxAngularVelocity = 180f;
    
    [Header("Water Physics")]
    [Tooltip("Drag applied when moving through water")]
    public float waterDrag = 2f;
    
    [Tooltip("Angular drag for rotation in water")]
    public float waterAngularDrag = 2f;
    
    [Header("Health Settings")]
    [Tooltip("Maximum health of the diver")]
    public int maxHealth = 100;
    
    [Tooltip("Current health of the diver")]
    public int currentHealth;
    
    [Tooltip("Invulnerability time after taking damage")]
    public float invulnerabilityTime = 1.5f;
    
    [Header("Animation Settings")]
    [Tooltip("Animator component for the diver")]
    public Animator diverAnimator;
    
    [Tooltip("Animation parameter for left foot")]
    public string leftFootParam = "LeftFoot";
    
    [Tooltip("Animation parameter for right foot")]
    public string rightFootParam = "RightFoot";
    
    [Tooltip("Animation parameter for both feet")]
    public string bothFeetParam = "BothFeet";
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for damage effect")]
    public ParticleSystem damageEffect;
    
    [Tooltip("Particle system for death effect")]
    public ParticleSystem deathEffect;
    
    [Tooltip("Light for the diver")]
    public Light2D diverLight;
    
    [Header("Audio")]
    [Tooltip("Sound when taking damage")]
    public AudioClip damageSound;
    
    [Tooltip("Sound when dying")]
    public AudioClip deathSound;
    
    [Header("Checkpoint Settings")]
    [Tooltip("Whether to use the checkpoint system for respawning")]
    public bool useCheckpointSystem = true;
    
    [Tooltip("Delay before respawning at checkpoint (in seconds)")]
    public float respawnDelay = 3f;
    
    [Header("Unstuck Settings")]
    [Tooltip("Key to press to get unstuck (teleport to last checkpoint)")]
    public KeyCode unstuckKey = KeyCode.F;
    
    [Tooltip("Cooldown time between unstuck attempts (in seconds)")]
    public float unstuckCooldown = 5f;
    
    [Tooltip("Whether to show a message when the player gets unstuck")]
    public bool showUnstuckMessage = true;
    
    [Tooltip("Message to display when the player gets unstuck")]
    public string unstuckMessage = "Teleported to last checkpoint";
    
    [Tooltip("Duration to display the unstuck message (in seconds)")]
    public float unstuckMessageDuration = 3f;
    
    private Rigidbody2D rb;
    private bool isLeftFlipperActive;
    private bool isRightFlipperActive;
    private float lastLeftFlipperTime;
    private float lastRightFlipperTime;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    private bool isDead = false;
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private bool wasLeftFlipperActive;
    private bool wasRightFlipperActive;
    private float unstuckCooldownTimer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("DiverMovement requires a Rigidbody2D component!");
            enabled = false;
            return;
        }

        // Set up water-like physics
        rb.drag = waterDrag;
        rb.angularDrag = waterAngularDrag;
        rb.gravityScale = 0.1f; // Slight downward force to simulate sinking
        
        // Initialize health
        currentHealth = maxHealth;
        
        // Get or add components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Get animator if not assigned
        if (diverAnimator == null) {
            diverAnimator = GetComponent<Animator>();
            if (diverAnimator == null) {
                Debug.LogWarning("No Animator component found on the diver. Animations will not work.");
            }
        }
        
        // Initialize flipper states
        wasLeftFlipperActive = false;
        wasRightFlipperActive = false;
        
        // Check if we should spawn at a checkpoint
        if (useCheckpointSystem && GameManager.Instance != null && GameManager.Instance.HasCheckpoint())
        {
            // Position will be set by GameManager during respawn
            Debug.Log("Player will spawn at checkpoint: " + GameManager.Instance.GetCheckpointPosition());
        }
    }

    private void Update()
    {
        // Don't process input if dead
        if (isDead) return;
        
        // Check for flipper input
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            isLeftFlipperActive = true;
            lastLeftFlipperTime = Time.time;
        }
        if (Input.GetMouseButtonUp(0))
        {
            isLeftFlipperActive = false;
        }

        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            isRightFlipperActive = true;
            lastRightFlipperTime = Time.time;
        }
        if (Input.GetMouseButtonUp(1))
        {
            isRightFlipperActive = false;
        }
        
        // Check for unstuck input
        if (Input.GetKeyDown(unstuckKey) && unstuckCooldownTimer <= 0f)
        {
            GetUnstuck();
        }
        
        // Update unstuck cooldown
        if (unstuckCooldownTimer > 0f)
        {
            unstuckCooldownTimer -= Time.deltaTime;
        }
        
        // Update animations
        UpdateAnimations();
        
        // Update invulnerability
        if (isInvulnerable) {
            invulnerabilityTimer += Time.deltaTime;
            if (invulnerabilityTimer >= invulnerabilityTime) {
                isInvulnerable = false;
                invulnerabilityTimer = 0f;
                
                // Reset sprite color
                if (spriteRenderer != null) {
                    spriteRenderer.color = Color.white;
                }
            } else {
                // Flash sprite during invulnerability
                if (spriteRenderer != null) {
                    float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                    spriteRenderer.color = new Color(1f, 1f, 1f, alpha);
                }
            }
        }
    }
    
    private void UpdateAnimations()
    {
        if (diverAnimator == null) return;
        
        // Check if flipper states have changed
        bool leftStateChanged = isLeftFlipperActive != wasLeftFlipperActive;
        bool rightStateChanged = isRightFlipperActive != wasRightFlipperActive;
        
        // If both flippers are active, trigger the combined animation
        if (isLeftFlipperActive && isRightFlipperActive) {
            // Reset individual foot animations
            diverAnimator.SetBool(leftFootParam, false);
            diverAnimator.SetBool(rightFootParam, false);
            
            // Trigger combined animation
            diverAnimator.SetBool(bothFeetParam, true);
        } 
        // If only left flipper is active
        else if (isLeftFlipperActive) {
            // Reset other animations
            diverAnimator.SetBool(rightFootParam, false);
            diverAnimator.SetBool(bothFeetParam, false);
            
            // Trigger left foot animation
            diverAnimator.SetBool(leftFootParam, true);
        } 
        // If only right flipper is active
        else if (isRightFlipperActive) {
            // Reset other animations
            diverAnimator.SetBool(leftFootParam, false);
            diverAnimator.SetBool(bothFeetParam, false);
            
            // Trigger right foot animation
            diverAnimator.SetBool(rightFootParam, true);
        } 
        // If no flippers are active, reset all animations
        else {
            diverAnimator.SetBool(leftFootParam, false);
            diverAnimator.SetBool(rightFootParam, false);
            diverAnimator.SetBool(bothFeetParam, false);
        }
        
        // Update previous states
        wasLeftFlipperActive = isLeftFlipperActive;
        wasRightFlipperActive = isRightFlipperActive;
    }

    private void FixedUpdate()
    {
        // Don't apply movement if dead
        if (isDead) return;
        
        ApplyMovement();
        ClampVelocities();
    }

    private void ApplyMovement()
    {
        Vector2 force = Vector2.zero;
        float torque = 0f;

        // Apply forces based on flipper state
        if (isLeftFlipperActive)
        {
            force += (Vector2)(transform.up * forwardForce);
            torque -= torqueForce;
        }

        if (isRightFlipperActive)
        {
            force += (Vector2)(transform.up * forwardForce);
            torque += torqueForce;
        }

        // Apply the forces
        rb.AddForce(force, ForceMode2D.Force);
        rb.AddTorque(torque, ForceMode2D.Force);
    }

    private void ClampVelocities()
    {
        // Clamp linear velocity
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity = rb.velocity.normalized * maxVelocity;
        }

        // Clamp angular velocity
        rb.angularVelocity = Mathf.Clamp(rb.angularVelocity, -maxAngularVelocity, maxAngularVelocity);
    }
    
    // Called when the diver takes damage
    public void TakeDamage(int damage)
    {
        // Don't take damage if invulnerable or dead
        if (isInvulnerable || isDead) return;
        
        // Apply damage
        currentHealth -= damage;
        
        // Play damage effects
        if (damageEffect != null) {
            damageEffect.Play();
        }
        
        // Play damage sound
        if (damageSound != null && audioSource != null) {
            audioSource.PlayOneShot(damageSound);
        }
        
        // Flash the sprite
        if (spriteRenderer != null) {
            spriteRenderer.color = Color.red;
        }
        
        // Check if dead
        if (currentHealth <= 0) {
            OnDeath();
        } else {
            // Become invulnerable
            isInvulnerable = true;
            invulnerabilityTimer = 0f;
        }
    }
    
    // Called when the diver dies
    public void OnDeath()
    {
        if (isDead) return;
        
        isDead = true;
        
        // Disable movement
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        // Play death effects
        if (deathEffect != null) {
            deathEffect.Play();
        }
        
        // Play death sound
        if (deathSound != null && audioSource != null) {
            audioSource.PlayOneShot(deathSound);
        }
        
        // Change sprite color
        if (spriteRenderer != null) {
            spriteRenderer.color = Color.gray;
        }
        
        // Disable light
        if (diverLight != null) {
            diverLight.intensity = 0.5f;
            diverLight.color = Color.red;
        }
        
        // Disable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders) {
            collider.enabled = false;
        }
        
        // Reset animations
        if (diverAnimator != null) {
            diverAnimator.SetBool(leftFootParam, false);
            diverAnimator.SetBool(rightFootParam, false);
            diverAnimator.SetBool(bothFeetParam, false);
        }
        
        // Use checkpoint system if enabled
        if (useCheckpointSystem && GameManager.Instance != null)
        {
            // Respawn at checkpoint after delay
            Invoke("RespawnAtCheckpoint", respawnDelay);
        }
        else
        {
            // Reload scene after delay
            Invoke("ReloadScene", respawnDelay);
        }
    }
    
    // Respawn at the current checkpoint
    private void RespawnAtCheckpoint()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RespawnPlayer(gameObject);
        }
        else
        {
            Debug.LogError("GameManager instance not found! Reloading scene instead.");
            ReloadScene();
        }
    }
    
    // Reset the player's state (called by GameManager during respawn)
    public void ResetPlayer()
    {
        // Reset health
        currentHealth = maxHealth;
        
        // Reset state variables
        isDead = false;
        isInvulnerable = false;
        invulnerabilityTimer = 0f;
        
        // Reset sprite color
        if (spriteRenderer != null) {
            spriteRenderer.color = Color.white;
        }
        
        // Reset light
        if (diverLight != null) {
            diverLight.intensity = 1f;
            diverLight.color = Color.white;
        }
        
        // Re-enable colliders
        Collider2D[] colliders = GetComponents<Collider2D>();
        foreach (Collider2D collider in colliders) {
            collider.enabled = true;
        }
        
        // Reset animations
        if (diverAnimator != null) {
            diverAnimator.SetBool(leftFootParam, false);
            diverAnimator.SetBool(rightFootParam, false);
            diverAnimator.SetBool(bothFeetParam, false);
        }
        
        Debug.Log("Player reset at checkpoint");
    }
    
    // Reload the current scene
    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Helper method to visualize the movement direction in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.up);
    }

    // Get unstuck by teleporting to the last checkpoint
    public void GetUnstuck()
    {
        // Check if GameManager exists and has a checkpoint
        if (GameManager.Instance != null && GameManager.Instance.HasCheckpoint())
        {
            // Get the checkpoint position
            Vector2 checkpointPosition = GameManager.Instance.GetCheckpointPosition();
            
            // Teleport the player
            transform.position = checkpointPosition;
            
            // Reset velocity
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            // Start cooldown
            unstuckCooldownTimer = unstuckCooldown;
            
            // Show message if enabled
            if (showUnstuckMessage)
            {
                Debug.Log(unstuckMessage);
                
                // Use NotificationManager if available
                if (NotificationManager.Instance != null)
                {
                    NotificationManager.Instance.ShowNotification(unstuckMessage, unstuckMessageDuration);
                }
            }
            
            Debug.Log("Player teleported to checkpoint: " + checkpointPosition);
        }
        else
        {
            Debug.LogWarning("No checkpoint available to teleport to!");
            
            // Show error message if NotificationManager is available
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowNotification("No checkpoint available!", 2f);
            }
        }
    }
} 