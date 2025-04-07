using UnityEngine;
using UnityEngine.Rendering.Universal;

public class UndercurrentTrap : MonoBehaviour
{
    [Header("Force Settings")]
    [Tooltip("Force applied in the undercurrent direction")]
    public float currentForce = 8f;
    
    [Tooltip("Maximum force that can be applied")]
    public float maxForce = 12f;
    
    [Tooltip("Direction of the undercurrent (in degrees)")]
    public float currentDirection = 0f;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for the undercurrent effect")]
    public ParticleSystem currentEffect;
    
    [Tooltip("Light for the undercurrent")]
    public Light2D currentLight;
    
    [Tooltip("Light color for the undercurrent")]
    public Color currentLightColor = new Color(0.2f, 0.5f, 1f, 1f);
    
    [Header("Audio")]
    [Tooltip("Ambient sound for the undercurrent")]
    public AudioClip ambientSound;
    
    [Tooltip("Volume of the ambient sound")]
    [Range(0f, 1f)]
    public float ambientVolume = 0.2f;
    
    private AudioSource audioSource;
    private Vector2 currentVector;
    private bool isPlayerInTrap = false;
    private DiverMovement playerDiver;
    private BoxCollider2D currentCollider;

    private void Start()
    {
        // Calculate the current vector based on direction
        float angleInRadians = currentDirection * Mathf.Deg2Rad;
        currentVector = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        
        // Get or add components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get the collider
        currentCollider = GetComponent<BoxCollider2D>();
        if (currentCollider == null) {
            currentCollider = gameObject.AddComponent<BoxCollider2D>();
            currentCollider.isTrigger = true;
            currentCollider.size = new Vector2(5f, 2f); // Default size
        }
        
        // Set up audio
        if (ambientSound != null) {
            audioSource.clip = ambientSound;
            audioSource.loop = true;
            audioSource.volume = ambientVolume;
            audioSource.Play();
        }
        
        // Set up light
        if (currentLight != null) {
            currentLight.color = currentLightColor;
        }
        
        // Start current effect
        if (currentEffect != null) {
            currentEffect.Play();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrap = true;
            playerDiver = other.GetComponent<DiverMovement>();
            
            // Increase current effect when player enters
            if (currentEffect != null) {
                var emission = currentEffect.emission;
                emission.rateOverTime = emission.rateOverTime.constant * 2;
            }
            
            Debug.Log("Player entered undercurrent");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrap = false;
            playerDiver = null;
            
            // Reset current effect when player exits
            if (currentEffect != null) {
                var emission = currentEffect.emission;
                emission.rateOverTime = emission.rateOverTime.constant / 2;
            }
            
            Debug.Log("Player exited undercurrent");
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isPlayerInTrap)
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Apply force in the current direction
                Vector2 forceVector = currentVector * currentForce;
                
                // Clamp the force
                if (forceVector.magnitude > maxForce)
                {
                    forceVector = forceVector.normalized * maxForce;
                }
                
                // Apply the force
                rb.AddForce(forceVector, ForceMode2D.Force);
                
                Debug.Log("Applying undercurrent force: " + forceVector);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw the undercurrent area
        Gizmos.color = new Color(0.2f, 0.5f, 1f, 0.3f);
        
        // Draw a rectangle to represent the undercurrent area
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
        }
        
        // Draw the current direction
        Gizmos.color = Color.blue;
        float angleInRadians = currentDirection * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
        Gizmos.DrawRay(transform.position, direction * currentForce * 0.5f);
    }
} 