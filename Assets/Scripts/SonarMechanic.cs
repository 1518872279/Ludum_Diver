using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class SonarMechanic : MonoBehaviour
{
    [Header("Sonar Settings")]
    [Tooltip("Maximum distance the sonar wave travels")]
    public float sonarRange = 20f;
    
    [Tooltip("Cooldown time between sonar activations")]
    public float cooldownTime = 5f;
    
    [Tooltip("Number of rays cast (360° / numRays = angle increments)")]
    public int numRays = 36;
    
    [Tooltip("Duration the ripple effect lasts")]
    public float rippleDuration = 2f;
    
    [Tooltip("Prefab for the ripple effect")]
    public GameObject ripplePrefab;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for the sonar wave")]
    public ParticleSystem sonarWaveEffect;
    
    [Tooltip("Light for the sonar wave")]
    public Light2D sonarLight;
    
    [Tooltip("Duration of the sonar light effect")]
    public float lightDuration = 0.5f;
    
    [Header("Audio")]
    [Tooltip("Sound when activating sonar")]
    public AudioClip sonarSound;
    
    [Tooltip("Volume of the sonar sound")]
    [Range(0f, 1f)]
    public float sonarVolume = 0.5f;
    
    [Header("Debug")]
    [Tooltip("Enable debug visualization")]
    public bool showDebugRays = true;
    
    [Tooltip("Layer mask for raycast detection")]
    public LayerMask detectionLayer = -1; // Default to all layers
    
    [Tooltip("Ignore player in detection")]
    public bool ignorePlayer = true;
    
    [Tooltip("Debug mode - show all detected objects")]
    public bool debugMode = false;
    
    [Tooltip("Force sonar to work regardless of player state")]
    public bool forceSonarWork = true;

    private bool canUseSonar = true;
    private Transform diverTransform;
    private AudioSource audioSource;
    private float cooldownTimer = 0f;
    private bool isOnCooldown = false;
    private int objectsDetected = 0;
    private List<GameObject> detectedObjects = new List<GameObject>();
    private bool isInitialized = false;

    void Awake()
    {
        // Initialize in Awake to ensure it's ready before any other scripts
        InitializeSonar();
    }

    void Start()
    {
        // Also initialize in Start as a backup
        if (!isInitialized) {
            InitializeSonar();
        }
    }
    
    void OnEnable()
    {
        // Re-initialize when the component is enabled
        if (!isInitialized) {
            InitializeSonar();
        }
    }
    
    void InitializeSonar()
    {
        // Get the diver's transform
        diverTransform = transform;
        
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Check if ripple prefab is assigned
        if (ripplePrefab == null) {
            Debug.LogWarning("Ripple prefab not assigned to SonarMechanic. Ripple effects will not appear.");
        }
        
        // Log initial setup
        Debug.Log("SonarMechanic initialized with range: " + sonarRange + ", rays: " + numRays);
        
        // Check for objects in the scene
        if (debugMode) {
            CheckForObjectsInScene();
        }
        
        isInitialized = true;
    }

    void Update()
    {
        // Ensure we're initialized
        if (!isInitialized) {
            InitializeSonar();
        }
        
        // Update cooldown timer
        if (isOnCooldown) {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= cooldownTime) {
                isOnCooldown = false;
                canUseSonar = true;
                cooldownTimer = 0f;
            }
        }
        
        // Check for sonar activation input (Space key)
        if (Input.GetKeyDown(KeyCode.Space) && canUseSonar) {
            ActivateSonar();
        }
    }

    void ActivateSonar()
    {
        // Set cooldown
        canUseSonar = false;
        isOnCooldown = true;
        cooldownTimer = 0f;
        objectsDetected = 0;
        detectedObjects.Clear();
        
        // Play sonar sound
        if (sonarSound != null && audioSource != null) {
            audioSource.PlayOneShot(sonarSound, sonarVolume);
        }
        
        // Play sonar wave effect
        if (sonarWaveEffect != null) {
            sonarWaveEffect.Play();
        }
        
        // Activate sonar light
        if (sonarLight != null) {
            sonarLight.enabled = true;
            StartCoroutine(DeactivateSonarLight());
        }
        
        // Create a layer mask that excludes the player layer
        LayerMask playerLayer = LayerMask.GetMask("Player");
        LayerMask sonarDetectionMask = detectionLayer & ~playerLayer;
        
        Debug.Log("Using sonar detection mask: " + sonarDetectionMask.value);
        
        // Emit sonar waves in all directions
        for (int i = 0; i < numRays; i++) {
            float angle = (360f / numRays) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            
            // Cast a ray from the diver's position in the given direction, ignoring player layer
            RaycastHit2D hit = Physics2D.Raycast(diverTransform.position, direction, sonarRange, sonarDetectionMask);
            
            // Debug ray visualization
            if (showDebugRays) {
                Debug.DrawRay(diverTransform.position, direction * sonarRange, Color.cyan, 0.5f);
            }
            
            if (hit.collider != null) {
                // Log hit information
                Debug.Log("Sonar hit object: " + hit.collider.gameObject.name + " at distance: " + hit.distance);
                
                objectsDetected++;
                
                // Add to detected objects list if not already there
                if (!detectedObjects.Contains(hit.collider.gameObject)) {
                    detectedObjects.Add(hit.collider.gameObject);
                }
                
                // Spawn a ripple effect at the hit point
                if (ripplePrefab != null) {
                    GameObject ripple = Instantiate(ripplePrefab, hit.point, Quaternion.identity);
                    Destroy(ripple, rippleDuration);
                    Debug.Log("Created ripple at: " + hit.point);
                } else {
                    Debug.LogWarning("No ripple prefab assigned, cannot create ripple effect");
                }
            }
        }
        
        // Log detection summary
        Debug.Log("Sonar activated. Detected " + objectsDetected + " objects.");
        
        // Log all detected objects in debug mode
        if (debugMode && detectedObjects.Count > 0) {
            Debug.Log("Detected objects:");
            foreach (GameObject obj in detectedObjects) {
                Debug.Log("- " + obj.name + " (Layer: " + LayerMask.LayerToName(obj.layer) + ")");
            }
        }
    }
    
    IEnumerator DeactivateSonarLight()
    {
        yield return new WaitForSeconds(lightDuration);
        if (sonarLight != null) {
            sonarLight.enabled = false;
        }
    }
    
    // Helper method to visualize the sonar range in the editor
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, sonarRange);
    }
    
    // Helper method to check for objects in the scene
    private void CheckForObjectsInScene()
    {
        Debug.Log("Checking for objects in the scene...");
        
        // Find all objects with colliders
        Collider2D[] colliders = FindObjectsOfType<Collider2D>();
        Debug.Log("Found " + colliders.Length + " objects with colliders in the scene");
        
        // Count objects by layer
        Dictionary<string, int> layerCounts = new Dictionary<string, int>();
        foreach (Collider2D collider in colliders) {
            string layerName = LayerMask.LayerToName(collider.gameObject.layer);
            if (!layerCounts.ContainsKey(layerName)) {
                layerCounts[layerName] = 0;
            }
            layerCounts[layerName]++;
        }
        
        // Log layer counts
        Debug.Log("Objects by layer:");
        foreach (KeyValuePair<string, int> pair in layerCounts) {
            Debug.Log("- " + pair.Key + ": " + pair.Value);
        }
        
        // Check if any objects are within sonar range
        int objectsInRange = 0;
        foreach (Collider2D collider in colliders) {
            float distance = Vector2.Distance(transform.position, collider.transform.position);
            if (distance <= sonarRange) {
                objectsInRange++;
                Debug.Log("Object in range: " + collider.gameObject.name + " at distance " + distance);
            }
        }
        
        Debug.Log("Found " + objectsInRange + " objects within sonar range");
    }
    
    // Public method to force sonar activation (can be called from other scripts)
    public void ForceActivateSonar()
    {
        if (isInitialized) {
            ActivateSonar();
        }
    }
} 