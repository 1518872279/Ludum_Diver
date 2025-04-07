using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [Tooltip("Transform representing the exact respawn position (defaults to this object's position)")]
    public Transform checkpointSpawnPoint;
    
    [Tooltip("Whether this checkpoint has been activated")]
    public bool isActivated = false;
    
    [Header("Visual Effects")]
    [Tooltip("Sprite renderer for the checkpoint (optional)")]
    public SpriteRenderer checkpointSprite;
    
    [Tooltip("Light for the checkpoint (optional)")]
    public Light2D checkpointLight;
    
    [Tooltip("Particle system for activation effect (optional)")]
    public ParticleSystem activationEffect;
    
    [Header("Audio")]
    [Tooltip("Sound to play when checkpoint is activated")]
    public AudioClip activationSound;
    
    [Tooltip("Audio source component (will be added if not present)")]
    public AudioSource audioSource;
    
    // Colors for active/inactive states
    [Header("Colors")]
    public Color inactiveColor = Color.gray;
    public Color activeColor = Color.green;
    
    private void Start()
    {
        // If no spawn point is set, use this object's transform
        if (checkpointSpawnPoint == null)
        {
            checkpointSpawnPoint = transform;
        }
        
        // Get or add audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // Initialize visual state
        UpdateVisualState();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint();
        }
    }
    
    // Activate the checkpoint
    public void ActivateCheckpoint()
    {
        if (!isActivated)
        {
            isActivated = true;
            
            // Update the respawn position in the GameManager
            GameManager.Instance.SetCheckpoint(checkpointSpawnPoint.position);
            
            // Play activation effects
            PlayActivationEffects();
            
            // Update visual state
            UpdateVisualState();
            
            Debug.Log("Checkpoint activated at: " + checkpointSpawnPoint.position);
        }
    }
    
    // Play visual and audio effects when checkpoint is activated
    private void PlayActivationEffects()
    {
        // Play particle effect
        if (activationEffect != null)
        {
            activationEffect.Play();
        }
        
        // Play sound
        if (activationSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(activationSound);
        }
    }
    
    // Update the visual state of the checkpoint
    private void UpdateVisualState()
    {
        // Update sprite color
        if (checkpointSprite != null)
        {
            checkpointSprite.color = isActivated ? activeColor : inactiveColor;
        }
        
        // Update light color
        if (checkpointLight != null)
        {
            checkpointLight.color = isActivated ? activeColor : inactiveColor;
        }
    }
    
    // Reset the checkpoint to inactive state
    public void ResetCheckpoint()
    {
        isActivated = false;
        UpdateVisualState();
    }
} 