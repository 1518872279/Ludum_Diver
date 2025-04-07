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
    private ParticleSystem.MainModule currentEffectMain;
    private ParticleSystem.EmissionModule currentEffectEmission;
    private float defaultEmissionRate = 0f;
    private bool isInitialized = false;

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
        
        // Set up particle system
        if (currentEffect != null) {
            currentEffectMain = currentEffect.main;
            currentEffectEmission = currentEffect.emission;
            
            // Store the default emission rate
            defaultEmissionRate = currentEffectEmission.rateOverTime.constant;
            
            // Start with emission rate at 0
            currentEffectEmission.rateOverTime = 0;
            
            // Make sure the particle system is ready to play
            currentEffect.Play();
            currentEffect.Pause();
        }
        
        isInitialized = true;
        
        // Start the activation cycle
        activationCoroutine = StartCoroutine(ActivationCycle());
    }
    
    private void UpdateCurrentVector()
    {
        float angleInRadians = currentDirection * Mathf.Deg2Rad;
        currentVector = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians));
    }
    
    private IEnumerator ActivationCycle()
    {
        // Wait for initialization to complete
        while (!isInitialized) {
            yield return null;
        }
        
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
    
    private void SetVisualEffects(bool active)
    {
        if (currentEffect != null)
        {
            if (active)
            {
                // Resume the particle system
                currentEffect.Play();
                
                // Set the emission rate to the default value
                currentEffectEmission.rateOverTime = defaultEmissionRate;
                
                Debug.Log("Undercurrent activated - Particle system playing with emission rate: " + defaultEmissionRate);
            }
            else
            {
                // Pause the particle system instead of stopping it
                currentEffect.Pause();
                
                // Set emission rate to 0
                currentEffectEmission.rateOverTime = 0;
                
                Debug.Log("Undercurrent deactivated - Particle system paused");
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
            
            // Increase current effect when player enters and undercurrent is active
            if (currentEffect != null && isActive) {
                currentEffectEmission.rateOverTime = defaultEmissionRate * 2;
                Debug.Log("Player entered undercurrent - Increased emission rate to: " + (defaultEmissionRate * 2));
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
            if (currentEffect != null && isActive) {
                currentEffectEmission.rateOverTime = defaultEmissionRate;
                Debug.Log("Player exited undercurrent - Reset emission rate to: " + defaultEmissionRate);
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
        
        // Reset visual effects
        SetVisualEffects(false);
    }
    
    // Public method to manually activate/deactivate the undercurrent
    public void SetActive(bool active)
    {
        isActive = active;
        SetVisualEffects(active);
    }
} 