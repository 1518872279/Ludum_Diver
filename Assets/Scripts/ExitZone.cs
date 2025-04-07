using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class ExitZone : MonoBehaviour
{
    [Header("Exit Settings")]
    [Tooltip("Time to wait before ending the game")]
    public float exitDelay = 2f;
    
    [Tooltip("Scene to load when exiting (leave empty to load next scene)")]
    public string nextSceneName = "";
    
    [Tooltip("Whether to show a victory message")]
    public bool showVictoryMessage = true;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for the exit effect")]
    public ParticleSystem exitEffect;
    
    [Tooltip("Light for the exit zone")]
    public Light2D exitLight;
    
    [Tooltip("Light color for the exit zone")]
    public Color exitLightColor = new Color(0f, 1f, 0.5f, 1f);
    
    [Header("Audio")]
    [Tooltip("Sound when entering the exit zone")]
    public AudioClip exitSound;
    
    [Tooltip("Volume of the exit sound")]
    [Range(0f, 1f)]
    public float exitVolume = 0.5f;
    
    private AudioSource audioSource;
    private bool isPlayerInZone = false;
    private bool hasExited = false;
    private BoxCollider2D exitCollider;
    private DiverMovement playerDiver;

    private void Start()
    {
        // Get or add components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get the collider
        exitCollider = GetComponent<BoxCollider2D>();
        if (exitCollider == null) {
            exitCollider = gameObject.AddComponent<BoxCollider2D>();
            exitCollider.isTrigger = true;
            exitCollider.size = new Vector2(3f, 3f); // Default size
        }
        
        // Set up light
        if (exitLight != null) {
            exitLight.color = exitLightColor;
        }
        
        // Start exit effect
        if (exitEffect != null) {
            exitEffect.Play();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !hasExited)
        {
            isPlayerInZone = true;
            playerDiver = other.GetComponent<DiverMovement>();
            
            // Play exit sound
            if (exitSound != null && audioSource != null) {
                audioSource.PlayOneShot(exitSound, exitVolume);
            }
            
            // Increase exit effect
            if (exitEffect != null) {
                var emission = exitEffect.emission;
                emission.rateOverTime = emission.rateOverTime.constant * 2;
            }
            
            // Show victory message
            if (showVictoryMessage) {
                Debug.Log("Player reached the exit! Victory!");
                // You can add UI elements here to show a victory message
            }
            
            // Start exit sequence
            StartCoroutine(ExitSequence());
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            playerDiver = null;
            
            // Reset exit effect
            if (exitEffect != null) {
                var emission = exitEffect.emission;
                emission.rateOverTime = emission.rateOverTime.constant / 2;
            }
        }
    }
    
    private IEnumerator ExitSequence()
    {
        // Mark that we've started exiting to prevent multiple triggers
        hasExited = true;
        
        // Wait for the exit delay
        yield return new WaitForSeconds(exitDelay);
        
        // Load the next scene
        if (!string.IsNullOrEmpty(nextSceneName)) {
            SceneManager.LoadScene(nextSceneName);
        } else {
            // Load the next scene in the build index
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings) {
                SceneManager.LoadScene(nextSceneIndex);
            } else {
                // If there's no next scene, reload the current scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw the exit zone area
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.3f);
        
        // Draw a rectangle to represent the exit zone area
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.offset, boxCollider.size);
        }
    }
} 