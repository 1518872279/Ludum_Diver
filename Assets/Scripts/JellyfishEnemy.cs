using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public enum JellyfishState { Idle, Attack }

public class JellyfishEnemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Range at which the jellyfish will detect and attack the player")]
    public float detectionRange = 8f;
    
    [Tooltip("Time to wait before returning to idle after losing the player")]
    public float returnToIdleDelay = 2f;
    
    [Header("Movement Settings")]
    [Tooltip("Base movement speed of the jellyfish")]
    public float baseSpeed = 3f;
    
    [Tooltip("Attack speed multiplier")]
    public float attackSpeedMultiplier = 1.5f;
    
    [Tooltip("Maximum speed the jellyfish can move at")]
    public float maxSpeed = 6f;
    
    [Tooltip("How quickly the jellyfish turns toward its target")]
    public float steeringSensitivity = 1.5f;
    
    [Header("Attack Settings")]
    [Tooltip("Duration of the attack")]
    public float attackDuration = 2f;
    
    [Tooltip("Cooldown between attacks")]
    public float attackCooldown = 5f;
    
    [Tooltip("Damage dealt to player on contact")]
    public int damage = 10;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for idle state")]
    public ParticleSystem idleEffect;
    
    [Tooltip("Particle system for attack state")]
    public ParticleSystem attackEffect;
    
    [Tooltip("Light for the jellyfish")]
    public Light2D jellyfishLight;
    
    [Header("Audio")]
    [Tooltip("Sound when jellyfish detects player")]
    public AudioClip detectionSound;
    
    [Tooltip("Sound when jellyfish attacks")]
    public AudioClip attackSound;
    
    [Tooltip("Ambient sound for jellyfish")]
    public AudioClip ambientSound;
    
    private JellyfishState currentState = JellyfishState.Idle;
    private Transform player;
    private float returnToIdleTimer = 0f;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private float cooldownTimer = 0f;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private Vector2 randomDirection;
    private float directionChangeTimer = 0f;
    private float directionChangeInterval = 3f;
    
    void Start() {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) {
            Debug.LogWarning("JellyfishEnemy: Player not found with tag 'Player'");
        }
        
        // Get or add components
        rb = GetComponent<Rigidbody2D>();
        if (rb == null) {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.drag = 1.5f;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Play ambient sound
        if (ambientSound != null && audioSource != null) {
            audioSource.clip = ambientSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Initialize random direction
        randomDirection = Random.insideUnitCircle.normalized;
        
        // Start idle effects
        if (idleEffect != null) {
            idleEffect.Play();
        }
    }
    
    void Update() {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Update timers
        if (isAttacking) {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackDuration) {
                isAttacking = false;
                attackTimer = 0f;
                cooldownTimer = 0f;
            }
        } else if (cooldownTimer < attackCooldown) {
            cooldownTimer += Time.deltaTime;
        }
        
        // State management
        switch (currentState) {
            case JellyfishState.Idle:
                // Check if player is in range to attack
                if (distanceToPlayer <= detectionRange && cooldownTimer >= attackCooldown) {
                    TransitionToAttack();
                } else {
                    IdleMovement();
                }
                break;
                
            case JellyfishState.Attack:
                // Check if player is out of range
                if (distanceToPlayer > detectionRange) {
                    returnToIdleTimer += Time.deltaTime;
                    if (returnToIdleTimer >= returnToIdleDelay) {
                        TransitionToIdle();
                    } else {
                        // Continue attacking for a bit after losing the player
                        AttackMovement();
                    }
                } else {
                    // Reset return timer when player is in range
                    returnToIdleTimer = 0f;
                    AttackMovement();
                }
                break;
        }
    }
    
    void TransitionToAttack() {
        if (currentState != JellyfishState.Attack) {
            currentState = JellyfishState.Attack;
            isAttacking = true;
            attackTimer = 0f;
            
            // Play detection sound
            if (detectionSound != null && audioSource != null) {
                audioSource.PlayOneShot(detectionSound);
            }
            
            // Switch effects
            if (idleEffect != null) {
                idleEffect.Stop();
            }
            if (attackEffect != null) {
                attackEffect.Play();
            }
            
            // Adjust light
            if (jellyfishLight != null) {
                jellyfishLight.intensity = 1.8f;
                jellyfishLight.color = new Color(0.5f, 0.3f, 1f);
            }
        }
    }
    
    void TransitionToIdle() {
        if (currentState != JellyfishState.Idle) {
            currentState = JellyfishState.Idle;
            
            // Switch effects
            if (attackEffect != null) {
                attackEffect.Stop();
            }
            if (idleEffect != null) {
                idleEffect.Play();
            }
            
            // Reset light
            if (jellyfishLight != null) {
                jellyfishLight.intensity = 1f;
                jellyfishLight.color = new Color(0.7f, 0.5f, 1f);
            }
        }
    }
    
    void IdleMovement() {
        // Change direction periodically
        directionChangeTimer += Time.deltaTime;
        if (directionChangeTimer >= directionChangeInterval) {
            randomDirection = Random.insideUnitCircle.normalized;
            directionChangeTimer = 0f;
        }
        
        // Apply movement
        rb.velocity = randomDirection * baseSpeed;
        
        // Clamp velocity
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        
        // Smoothly rotate towards movement direction
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), steeringSensitivity * Time.deltaTime);
    }
    
    void AttackMovement() {
        if (player == null) return;
        
        // Calculate direction to player
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Move toward the player with increased speed
        rb.velocity = direction * (baseSpeed * attackSpeedMultiplier);
        
        // Clamp velocity
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        
        // Smoothly rotate towards the player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), steeringSensitivity * Time.deltaTime);
    }
    
    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            // Play attack sound
            if (attackSound != null && audioSource != null) {
                audioSource.PlayOneShot(attackSound);
            }
            
            // Get the player's DiverMovement script
            DiverMovement diverMovement = collision.gameObject.GetComponent<DiverMovement>();
            if (diverMovement != null) {
                // Apply damage to player
                // This would need to be implemented in the DiverMovement script
                // For example: diverMovement.TakeDamage(damage);
            }
            
            Debug.Log("Player hit by jellyfish!");
        }
    }
    
    // Visualize the detection range in the editor
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
} 