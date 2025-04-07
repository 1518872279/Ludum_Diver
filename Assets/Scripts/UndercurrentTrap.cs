using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class UndercurrentTrap : MonoBehaviour
{
    [Header("Force Settings")]
    [Tooltip("Force applied in the undercurrent direction")]
    public float currentForce = 8f;
    
    [Tooltip("Maximum force that can be applied")]
    public float maxForce = 12f;
    
    [Tooltip("Direction of the undercurrent (in degrees)")]
    public float currentDirection = 0f;
    
    [Header("Timing Settings")]
    [Tooltip("Time the undercurrent is active")]
    public float activeTime = 3f;
    
    [Tooltip("Time the undercurrent is inactive")]
    public float inactiveTime = 2f;
    
    [Tooltip("Time to rotate the undercurrent direction")]
    public float rotationInterval = 5f;
    
    [Tooltip("Amount to rotate in degrees")]
    public float rotationAmount = 45f;
    
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
    private bool isActive = true;
    private Coroutine activationCoroutine;
    private Coroutine rotationCoroutine;

    private void Start()
    {
        // Calculate the current vector based on direction
        UpdateCurrentVector();
        
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
        
        // Start the activation cycle
        activationCoroutine = StartCoroutine(ActivationCycle());
        
        // Start the rotation cycle
        rotationCoroutine = StartCoroutine(RotationCycle());
    }
    
    private void UpdateCurrentVector()
    {
        float angleInRadians = currentDirection * Mathf.Deg2Rad;
        currentVector = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }
    
    private IEnumerator ActivationCycle()
    {
        while (true)
        {
            // Active phase
            isActive = true;
            SetVisualEffects(true);
            yield return new WaitForSeconds(activeTime);
            
            // Inactive phase
            isActive = false;
            SetVisualEffects(false);
            yield return new WaitForSeconds(inactiveTime);
        }
    }
    
    private IEnumerator RotationCycle()
    {
        while (true)
        {
            yield return new WaitForSeconds(rotationInterval);
            
            // Rotate the direction
            currentDirection += rotationAmount;
            if (currentDirection >= 360f)
            {
                currentDirection -= 360f;
            }
            
            UpdateCurrentVector();
            Debug.Log("Undercurrent rotated to: " + currentDirection + " degrees");
        }
    }
    
    private void SetVisualEffects(bool active)
    {
        if (currentEffect != null)
        {
            if (active)
            {
                currentEffect.Play();
            }
            else
            {
                currentEffect.Stop();
            }
        }
        
        if (currentLight != null)
        {
            currentLight.enabled = active;
        }
        
        if (audioSource != null)
        {
            audioSource.volume = active ? ambientVolume : 0f;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrap = true;
            playerDiver = other.GetComponent<DiverMovement>();
            
            // Increase current effect when player enters
            if (currentEffect != null && isActive) {
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
        if (other.CompareTag("Player") && isPlayerInTrap && isActive)
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
    
    private void OnDisable()
    {
        // Stop all coroutines when disabled
        if (activationCoroutine != null)
        {
            StopCoroutine(activationCoroutine);
        }
        
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        
        // Reset visual effects
        SetVisualEffects(false);
    }
} 