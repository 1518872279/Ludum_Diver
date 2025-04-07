using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public enum SharkState { Patrol, Chase }

public class SharkEnemy : MonoBehaviour
{
    [Header("Detection Settings")]
    [Tooltip("Range at which the shark will detect and chase the player")]
    public float chaseRange = 12f;
    
    [Tooltip("Time to wait before returning to patrol after losing the player")]
    public float returnToPatrolDelay = 3f;
    
    [Header("Movement Settings")]
    [Tooltip("Movement speed of the shark")]
    public float speed = 5f;
    
    [Tooltip("How quickly the shark turns toward its target")]
    public float steeringSensitivity = 2f;
    
    [Tooltip("Maximum speed the shark can move at")]
    public float maxSpeed = 8f;
    
    [Header("Patrol Settings")]
    [Tooltip("List of patrol waypoints")]
    public List<Transform> patrolNodes;
    
    [Tooltip("Distance threshold to consider a waypoint reached")]
    public float waypointReachedDistance = 1f;
    
    [Tooltip("Time to wait at each waypoint")]
    public float waitTimeAtWaypoint = 1f;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for chase mode")]
    public ParticleSystem chaseEffect;
    
    [Tooltip("Light for the shark")]
    public Light2D sharkLight;
    
    [Header("Audio")]
    [Tooltip("Sound when shark detects player")]
    public AudioClip detectionSound;
    
    [Tooltip("Sound when shark attacks")]
    public AudioClip attackSound;
    
    private int currentNodeIndex = 0;
    private SharkState currentState = SharkState.Patrol;
    private Transform player;
    private float waitTimer = 0f;
    private float returnToPatrolTimer = 0f;
    private bool isWaiting = false;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    
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
            rb.drag = 1f;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Initialize state
        currentState = SharkState.Patrol;
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
                    Patrol();
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
                        ChasePlayer();
                    }
                } else {
                    // Reset return timer when player is in range
                    returnToPatrolTimer = 0f;
                    ChasePlayer();
                }
                break;
        }
    }
    
    void TransitionToChase() {
        if (currentState != SharkState.Chase) {
            currentState = SharkState.Chase;
            
            // Play detection sound
            if (detectionSound != null && audioSource != null) {
                audioSource.clip = detectionSound;
                audioSource.Play();
            }
            
            // Activate chase effects
            if (chaseEffect != null) {
                chaseEffect.Play();
            }
            
            // Adjust light
            if (sharkLight != null) {
                sharkLight.intensity = 1.5f;
                sharkLight.color = new Color(1f, 0.3f, 0.3f);
            }
        }
    }
    
    void TransitionToPatrol() {
        if (currentState != SharkState.Patrol) {
            currentState = SharkState.Patrol;
            
            // Stop chase effects
            if (chaseEffect != null) {
                chaseEffect.Stop();
            }
            
            // Reset light
            if (sharkLight != null) {
                sharkLight.intensity = 1f;
                sharkLight.color = Color.white;
            }
        }
    }
    
    void Patrol() {
        if (patrolNodes.Count == 0) return;
        
        // If waiting at a waypoint
        if (isWaiting) {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtWaypoint) {
                isWaiting = false;
                waitTimer = 0f;
                currentNodeIndex = (currentNodeIndex + 1) % patrolNodes.Count;
            }
            return;
        }
        
        Transform targetNode = patrolNodes[currentNodeIndex];
        if (targetNode == null) return;
        
        // Move toward the target node
        Vector2 direction = (targetNode.position - transform.position).normalized;
        rb.velocity = direction * speed;
        
        // Clamp velocity
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        
        // Smoothly rotate towards the patrol node
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), steeringSensitivity * Time.deltaTime);
        
        // Check if reached the waypoint
        if (Vector2.Distance(transform.position, targetNode.position) < waypointReachedDistance) {
            isWaiting = true;
            waitTimer = 0f;
        }
    }
    
    void ChasePlayer() {
        if (player == null) return;
        
        // Calculate direction to player
        Vector2 direction = (player.position - transform.position).normalized;
        
        // Move toward the player
        rb.velocity = direction * speed;
        
        // Clamp velocity
        if (rb.velocity.magnitude > maxSpeed) {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        
        // Smoothly rotate towards the player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), steeringSensitivity * Time.deltaTime);
    }
    
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            // Play attack sound
            if (attackSound != null && audioSource != null) {
                audioSource.clip = attackSound;
                audioSource.Play();
            }
            
            // Get the player's DiverMovement script
            DiverMovement diverMovement = collision.gameObject.GetComponent<DiverMovement>();
            if (diverMovement != null) {
                // Call the player's death method if it exists
                // This would need to be implemented in the DiverMovement script
                // For example: diverMovement.OnDeath();
            }
            
            Debug.Log("Player killed by shark!");
        }
    }
    
    // Visualize the chase range in the editor
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // Draw patrol path
        if (patrolNodes != null && patrolNodes.Count > 0) {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolNodes.Count; i++) {
                if (patrolNodes[i] != null) {
                    Gizmos.DrawSphere(patrolNodes[i].position, 0.5f);
                    if (i < patrolNodes.Count - 1 && patrolNodes[i+1] != null) {
                        Gizmos.DrawLine(patrolNodes[i].position, patrolNodes[i+1].position);
                    }
                }
            }
            // Connect last node to first node
            if (patrolNodes.Count > 0 && patrolNodes[0] != null && patrolNodes[patrolNodes.Count-1] != null) {
                Gizmos.DrawLine(patrolNodes[patrolNodes.Count-1].position, patrolNodes[0].position);
            }
        }
    }
} 