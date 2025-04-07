using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SharpRocksTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt to the player")]
    public int damage = 25;
    
    [Tooltip("Time between damage ticks")]
    public float damageInterval = 0.5f;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for when player hits the rocks")]
    public ParticleSystem hitEffect;
    
    [Tooltip("Light for the rocks")]
    public Light2D rocksLight;
    
    [Tooltip("Light color for the rocks")]
    public Color rocksLightColor = new Color(1f, 0.3f, 0.3f, 1f);
    
    [Header("Audio")]
    [Tooltip("Sound played when player hits the rocks")]
    public AudioClip hitSound;
    
    [Tooltip("Volume of the hit sound")]
    [Range(0f, 1f)]
    public float hitVolume = 0.5f;
    
    private AudioSource audioSource;
    private float lastDamageTime;
    private bool isPlayerInTrap = false;
    private DiverMovement playerDiver;
    private PolygonCollider2D rocksCollider;

    private void Start()
    {
        // Get or add components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Get the collider
        rocksCollider = GetComponent<PolygonCollider2D>();
        if (rocksCollider == null) {
            rocksCollider = gameObject.AddComponent<PolygonCollider2D>();
            rocksCollider.isTrigger = true;
            
            // Create a simple triangle shape if no collider exists
            Vector2[] points = new Vector2[3];
            points[0] = new Vector2(0, 0);
            points[1] = new Vector2(1, 0);
            points[2] = new Vector2(0.5f, 1);
            rocksCollider.points = points;
        }
        
        // Set up light
        if (rocksLight != null) {
            rocksLight.color = rocksLightColor;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrap = true;
            playerDiver = other.GetComponent<DiverMovement>();
            
            // Play hit effect
            if (hitEffect != null) {
                hitEffect.Play();
            }
            
            // Play hit sound
            if (hitSound != null && audioSource != null) {
                audioSource.PlayOneShot(hitSound, hitVolume);
            }
            
            // Deal initial damage
            DealDamage();
            
            Debug.Log("Player entered sharp rocks");
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrap = false;
            playerDiver = null;
            
            Debug.Log("Player exited sharp rocks");
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isPlayerInTrap)
        {
            // Check if enough time has passed since last damage
            if (Time.time - lastDamageTime >= damageInterval)
            {
                DealDamage();
            }
        }
    }
    
    private void DealDamage()
    {
        if (playerDiver != null)
        {
            playerDiver.TakeDamage(damage);
            lastDamageTime = Time.time;
            
            // Play hit effect
            if (hitEffect != null) {
                hitEffect.Play();
            }
            
            // Play hit sound
            if (hitSound != null && audioSource != null) {
                audioSource.PlayOneShot(hitSound, hitVolume);
            }
            
            Debug.Log("Dealt " + damage + " damage to player");
        }
    }
    
    private void OnDrawGizmos()
    {
        // Draw the rocks area
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.3f);
        
        // Draw a polygon to represent the rocks
        PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D>();
        if (polygonCollider != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            for (int i = 0; i < polygonCollider.points.Length; i++)
            {
                int nextIndex = (i + 1) % polygonCollider.points.Length;
                Gizmos.DrawLine(polygonCollider.points[i], polygonCollider.points[nextIndex]);
            }
        }
    }
} 