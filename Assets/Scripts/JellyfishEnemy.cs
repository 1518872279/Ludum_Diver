using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public enum JellyfishType
{
    Vertical,
    Horizontal
}

public class JellyfishEnemy : MonoBehaviour
{
    [Header("Jellyfish Type")]
    [Tooltip("Type of jellyfish movement pattern")]
    public JellyfishType jellyfishType = JellyfishType.Vertical;

    [Header("Movement Settings")]
    [Tooltip("Base movement speed")]
    public float baseSpeed = 2f;
    
    [Tooltip("Maximum movement speed")]
    public float maxSpeed = 5f;
    
    [Tooltip("Steering sensitivity for movement")]
    public float steeringSensitivity = 0.5f;
    
    [Tooltip("Movement range from starting position")]
    public float movementRange = 5f;

    [Header("Attack Settings")]
    [Tooltip("Damage dealt to player on contact")]
    public int damage = 100; // Instant kill damage

    [Header("Visual Effects")]
    [Tooltip("Particle system for movement trail")]
    public ParticleSystem trailEffect;
    
    [Tooltip("Light component for the jellyfish")]
    public Light2D jellyfishLight;
    
    [Tooltip("Pulse speed of the light")]
    public float lightPulseSpeed = 1f;
    
    [Tooltip("Minimum light intensity")]
    public float minLightIntensity = 0.5f;
    
    [Tooltip("Maximum light intensity")]
    public float maxLightIntensity = 1.5f;

    [Header("Audio")]
    [Tooltip("Ambient sound for the jellyfish")]
    public AudioClip ambientSound;
    
    [Tooltip("Sound when hitting player")]
    public AudioClip hitSound;
    
    [Tooltip("Volume of the ambient sound")]
    [Range(0f, 1f)]
    public float ambientVolume = 0.3f;

    private Rigidbody2D rb;
    private Vector2 startPosition;
    private float movementTimer = 0f;
    private AudioSource audioSource;
    private float initialLightIntensity;
    private CircleCollider2D jellyfishCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("JellyfishEnemy requires a Rigidbody2D component!");
            enabled = false;
            return;
        }

        // Store initial position
        startPosition = transform.position;
        
        // Get or add components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get or add collider
        jellyfishCollider = GetComponent<CircleCollider2D>();
        if (jellyfishCollider == null) {
            jellyfishCollider = gameObject.AddComponent<CircleCollider2D>();
            jellyfishCollider.isTrigger = true;
            jellyfishCollider.radius = 0.5f; // Default radius
        }
        
        // Set up audio
        if (ambientSound != null) {
            audioSource.clip = ambientSound;
            audioSource.loop = true;
            audioSource.volume = ambientVolume;
            audioSource.Play();
        }
        
        // Store initial light intensity
        if (jellyfishLight != null) {
            initialLightIntensity = jellyfishLight.intensity;
        }
    }

    private void Update()
    {
        // Update movement timer
        movementTimer += Time.deltaTime;
        
        // Pulse the light
        if (jellyfishLight != null) {
            float pulse = Mathf.PingPong(Time.time * lightPulseSpeed, 1f);
            jellyfishLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, pulse);
        }
    }

    private void FixedUpdate()
    {
        MoveJellyfish();
    }

    private void MoveJellyfish()
    {
        // Calculate target position based on jellyfish type
        Vector2 targetPosition = startPosition;
        float offset = Mathf.Sin(movementTimer * baseSpeed) * movementRange;
        
        if (jellyfishType == JellyfishType.Vertical)
        {
            targetPosition.y += offset;
        }
        else // Horizontal
        {
            targetPosition.x += offset;
        }

        // Calculate direction to target
        Vector2 direction = (targetPosition - (Vector2)transform.position).normalized;
        
        // Apply movement force
        Vector2 force = direction * baseSpeed;
        rb.AddForce(force, ForceMode2D.Force);
        
        // Clamp velocity
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        
        // Rotate to face movement direction
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float currentRotation = transform.eulerAngles.z;
        float newRotation = Mathf.LerpAngle(currentRotation, angle, steeringSensitivity * Time.fixedDeltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newRotation);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jellyfish hit player!");
            
            // Play hit sound
            if (hitSound != null && audioSource != null) {
                audioSource.PlayOneShot(hitSound);
            }
            
            // Get the player's DiverMovement component
            DiverMovement player = other.GetComponent<DiverMovement>();
            if (player != null)
            {
                // Apply damage to player
                player.TakeDamage(damage);
                Debug.Log("Jellyfish dealt " + damage + " damage to player");
            }
            else
            {
                Debug.LogWarning("Player object does not have DiverMovement component!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw movement range
        Gizmos.color = Color.cyan;
        if (jellyfishType == JellyfishType.Vertical)
        {
            Gizmos.DrawLine(
                transform.position + Vector3.up * movementRange,
                transform.position + Vector3.down * movementRange
            );
        }
        else
        {
            Gizmos.DrawLine(
                transform.position + Vector3.right * movementRange,
                transform.position + Vector3.left * movementRange
            );
        }
        
        // Draw collider
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, GetComponent<CircleCollider2D>()?.radius ?? 0.5f);
    }
} 