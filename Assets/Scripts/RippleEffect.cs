using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RippleEffect : MonoBehaviour
{
    [Header("Ripple Settings")]
    [Tooltip("Duration of the ripple effect")]
    public float duration = 2f;
    
    [Tooltip("Initial scale of the ripple")]
    public float initialScale = 0.1f;
    
    [Tooltip("Maximum scale of the ripple")]
    public float maxScale = 2f;
    
    [Tooltip("Color of the ripple")]
    public Color rippleColor = new Color(0f, 1f, 1f, 0.5f);
    
    [Header("Visual Effects")]
    [Tooltip("Sprite renderer for the ripple")]
    public SpriteRenderer rippleSprite;
    
    [Tooltip("Light for the ripple")]
    public Light2D rippleLight;
    
    [Tooltip("Particle system for the ripple")]
    public ParticleSystem rippleParticles;
    
    private float startTime;
    private float endTime;
    
    void Start()
    {
        // Set up the ripple
        startTime = Time.time;
        endTime = startTime + duration;
        
        // Initialize components
        if (rippleSprite == null) {
            rippleSprite = GetComponent<SpriteRenderer>();
        }
        
        // Set initial scale
        transform.localScale = new Vector3(initialScale, initialScale, 1f);
        
        // Set color
        if (rippleSprite != null) {
            rippleSprite.color = rippleColor;
        }
        
        // Play particle effect
        if (rippleParticles != null) {
            rippleParticles.Play();
        }
    }
    
    void Update()
    {
        // Calculate progress
        float progress = (Time.time - startTime) / duration;
        
        // Scale the ripple
        float currentScale = Mathf.Lerp(initialScale, maxScale, progress);
        transform.localScale = new Vector3(currentScale, currentScale, 1f);
        
        // Fade out the ripple
        if (rippleSprite != null) {
            Color currentColor = rippleSprite.color;
            currentColor.a = Mathf.Lerp(rippleColor.a, 0f, progress);
            rippleSprite.color = currentColor;
        }
        
        // Fade out the light
        if (rippleLight != null) {
            rippleLight.intensity = Mathf.Lerp(1f, 0f, progress);
        }
        
        // Destroy when done
        if (Time.time >= endTime) {
            Destroy(gameObject);
        }
    }
} 