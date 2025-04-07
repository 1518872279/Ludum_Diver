using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WhirlpoolTrap : MonoBehaviour
{
    [Header("Force Settings")]
    [Tooltip("Force that pulls the player toward the center")]
    public float dragForce = 10f;
    
    [Tooltip("Force that creates circular movement")]
    public float circularForce = 5f;
    
    [Tooltip("Maximum force that can be applied")]
    public float maxForce = 15f;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for the whirlpool effect")]
    public ParticleSystem whirlpoolEffect;
    
    [Tooltip("Light for the whirlpool")]
    public Light2D whirlpoolLight;
    
    [Tooltip("Pulse speed of the light")]
    public float lightPulseSpeed = 1.5f;
    
    [Tooltip("Minimum light intensity")]
    public float minLightIntensity = 0.5f;
    
    [Tooltip("Maximum light intensity")]
    public float maxLightIntensity = 2f;
    
    [Header("Audio")]
    [Tooltip("Ambient sound for the whirlpool")]
    public AudioClip ambientSound;
    
    [Tooltip("Volume of the ambient sound")]
    [Range(0f, 1f)]
    public float ambientVolume = 0.3f;
    
    private AudioSource audioSource;
    private float initialLightIntensity;
    private bool isPlayerInTrap = false;
    private DiverMovement playerDiver;
    private CircleCollider2D whirlpoolCollider;

    private void Start()
    {
        // Get or add components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get the collider
        whirlpoolCollider = GetComponent<CircleCollider2D>();
        if (whirlpoolCollider == null) {
            whirlpoolCollider = gameObject.AddComponent<CircleCollider2D>();
            whirlpoolCollider.isTrigger = true;
            whirlpoolCollider.radius = 2f; // Default radius
        }
        
        // Set up audio
        if (ambientSound != null) {
            audioSource.clip = ambientSound;
            audioSource.loop = true;
            audioSource.volume = ambientVolume;
            audioSource.Play();
        }
        
        // Store initial light intensity
        if (whirlpoolLight != null) {
            initialLightIntensity = whirlpoolLight.intensity;
        }
        
        // Start whirlpool effect
        if (whirlpoolEffect != null) {
            whirlpoolEffect.Play();
        }
    }
    
    private void Update()
    {
        // Pulse the light
        if (whirlpoolLight != null) {
            float pulse = Mathf.PingPong(Time.time * lightPulseSpeed, 1f);
            whirlpoolLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, pulse);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrap = true;
            playerDiver = other.GetComponent<DiverMovement>();
            
            // Increase whirlpool effect when player enters
            if (whirlpoolEffect != null) {
                var emission = whirlpoolEffect.emission;
                emission.rateOverTime = emission.rateOverTime.constant * 2;
            }
            
            Debug.Log("Player entered whirlpool");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrap = false;
            playerDiver = null;
            
            // Reset whirlpool effect when player exits
            if (whirlpoolEffect != null) {
                var emission = whirlpoolEffect.emission;
                emission.rateOverTime = emission.rateOverTime.constant / 2;
            }
            
            Debug.Log("Player exited whirlpool");
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isPlayerInTrap)
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Calculate the direction to the trap center
                Vector2 directionToCenter = ((Vector2)transform.position - rb.position).normalized;
                
                // Apply drag force to pull the player towards the center
                Vector2 dragForceVector = directionToCenter * dragForce;
                
                // Calculate a perpendicular vector to create circular motion
                Vector2 perpendicular = new Vector2(-directionToCenter.y, directionToCenter.x);
                Vector2 circularForceVector = perpendicular * circularForce;
                
                // Combine forces
                Vector2 totalForce = dragForceVector + circularForceVector;
                
                // Clamp the total force
                if (totalForce.magnitude > maxForce)
                {
                    totalForce = totalForce.normalized * maxForce;
                }
                
                // Apply the force
                rb.AddForce(totalForce, ForceMode2D.Force);
                
                Debug.Log("Applying whirlpool force: " + totalForce);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw the whirlpool area
        Gizmos.color = new Color(0.5f, 0.2f, 0.8f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, GetComponent<CircleCollider2D>()?.radius ?? 1f);
        
        // Draw force direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector2.right * dragForce * 0.1f);
        
        // Draw circular force
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector2.up * circularForce * 0.1f);
    }
} 