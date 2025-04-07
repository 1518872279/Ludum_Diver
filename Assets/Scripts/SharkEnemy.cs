using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public enum SharkState { Patrol, Chase }

public class SharkEnemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Range at which the shark will detect and chase the player")]
    public float chaseRange = 10f;
    
    [Tooltip("Time to wait before returning to patrol after losing the player")]
    public float returnToPatrolDelay = 2f;
    
    [Header("Movement Settings")]
    [Tooltip("Base movement speed of the shark")]
    public float baseSpeed = 5f;
    
    [Tooltip("Chase speed multiplier")]
    public float chaseSpeedMultiplier = 1.5f;
    
    [Tooltip("Maximum speed the shark can move at")]
    public float maxSpeed = 8f;
    
    [Tooltip("How quickly the shark turns toward its target")]
    public float steeringSensitivity = 1.5f;
    
    [Header("Patrol Settings")]
    [Tooltip("List of waypoints for the shark to patrol between")]
    public List<Transform> patrolNodes = new List<Transform>();
    
    [Tooltip("Time to wait at each patrol node")]
    public float patrolWaitTime = 2f;
    
    [Header("Attack Settings")]
    [Tooltip("Damage dealt to player on contact")]
    public int damage = 100; // Instant kill damage
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for patrol state")]
    public ParticleSystem patrolEffect;
    
    [Tooltip("Particle system for chase state")]
    public ParticleSystem chaseEffect;
    
    [Tooltip("Light for the shark")]
    public Light2D sharkLight;
    
    [Header("Audio")]
    [Tooltip("Sound when shark detects player")]
    public AudioClip detectionSound;
    
    [Tooltip("Sound when shark attacks")]
    public AudioClip attackSound;
    
    [Tooltip("Ambient sound for shark")]
    public AudioClip ambientSound;
    
    private SharkState currentState = SharkState.Patrol;
    private Transform player;
    private int currentNodeIndex = 0;
    private float waitTimer = 0f;
    private float returnToPatrolTimer = 0f;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private Vector2 randomDirection;
    private float directionChangeTimer = 0f;
    private float directionChangeInterval = 3f;

    void Start() {
        // Find the player
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) {
            Debug.LogWarning("SharkEnemy: Player not found with tag 'Player'");
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
        
        // Start patrol effects
        if (patrolEffect != null) {
            patrolEffect.Play();
        }
    }
    
    void Update() {
        if (player == null) return;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // State management
        switch (currentState) {
            case SharkState.Patrol:
                // Check if player is in range to chase
                if (distanceToPlayer <= chaseRange) {
                    TransitionToChase();
                } else {
                    PatrolMovement();
                }
                break;
                
            case SharkState.Chase:
                // Check if player is out of range
                if (distanceToPlayer > chaseRange) {
                    returnToPatrolTimer += Time.deltaTime;
                    if (returnToPatrolTimer >= returnToPatrolDelay) {
                        TransitionToPatrol();
                    } else {
                        // Continue chasing for a bit after losing the player
                        ChaseMovement();
                    }
                } else {
                    // Reset return timer when player is in range
                    returnToPatrolTimer = 0f;
                    ChaseMovement();
                }
                break;
        }
    }
    
    void TransitionToChase() {
        if (currentState != SharkState.Chase) {
            currentState = SharkState.Chase;
            
            // Play detection sound
            if (detectionSound != null && audioSource != null) {
                audioSource.PlayOneShot(detectionSound);
            }
            
            // Switch effects
            if (patrolEffect != null) {
                patrolEffect.Stop();
            }
            if (chaseEffect != null) {
                chaseEffect.Play();
            }
            
            // Adjust light
            if (sharkLight != null) {
                sharkLight.intensity = 1.8f;
                sharkLight.color = new Color(1f, 0.3f, 0.3f);
            }
        }
    }
    
    void TransitionToPatrol() {
        if (currentState != SharkState.Patrol) {
            currentState = SharkState.Patrol;
            
            // Switch effects
            if (chaseEffect != null) {
                chaseEffect.Stop();
            }
            if (patrolEffect != null) {
                patrolEffect.Play();
            }
            
            // Reset light
            if (sharkLight != null) {
                sharkLight.intensity = 1f;
                sharkLight.color = new Color(0.7f, 0.7f, 0.7f);
            }
        }
    }
    
    void PatrolMovement() {
        if (patrolNodes.Count == 0) {
            // Random movement if no patrol nodes
            directionChangeTimer += Time.deltaTime;
            if (directionChangeTimer >= directionChangeInterval) {
                randomDirection = Random.insideUnitCircle.normalized;
                directionChangeTimer = 0f;
            }
            
            // Apply movement
            rb.velocity = randomDirection * baseSpeed;
        } else {
            // Move to next patrol node
            Transform currentNode = patrolNodes[currentNodeIndex];
            if (currentNode != null) {
                Vector2 direction = (currentNode.position - transform.position).normalized;
                rb.velocity = direction * baseSpeed;
                
                // Check if reached node
                if (Vector2.Distance(transform.position, currentNode.position) < 0.5f) {
                    waitTimer += Time.deltaTime;
                    if (waitTimer >= patrolWaitTime) {
                        waitTimer = 0f;
                        currentNodeIndex = (currentNodeIndex + 1) % patrolNodes.Count;
                    }
                }
            }
        }
        
        // Clamp velocity
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        
        // Smoothly rotate towards movement direction
        float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), steeringSensitivity * Time.deltaTime);
    }
    
    void ChaseMovement() {
        if (player == null) return;
        
        // Calculate direction to player
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Move toward the player with increased speed
        rb.velocity = direction * (baseSpeed * chaseSpeedMultiplier);
        
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
                diverMovement.TakeDamage(damage);
            }
        }
    }
    
    // Visualize the detection range in the editor
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Draw patrol path
        if (patrolNodes.Count > 0) {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < patrolNodes.Count; i++) {
                if (patrolNodes[i] != null) {
                    Gizmos.DrawSphere(patrolNodes[i].position, 0.5f);
                    if (i < patrolNodes.Count - 1 && patrolNodes[i + 1] != null) {
                        Gizmos.DrawLine(patrolNodes[i].position, patrolNodes[i + 1].position);
                    }
                }
            }
            if (patrolNodes.Count > 0 && patrolNodes[0] != null) {
                Gizmos.DrawLine(patrolNodes[patrolNodes.Count - 1].position, patrolNodes[0].position);
            }
        }
    }
} 