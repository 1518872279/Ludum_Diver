using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    [Tooltip("Time between portal activations")]
    public float activationInterval = 10f;
    
    [Tooltip("Duration of portal activation")]
    public float activationDuration = 3f;
    
    [Tooltip("Chance to spawn a monster when activated")]
    [Range(0f, 1f)]
    public float monsterSpawnChance = 0.7f;
    
    [Header("Visual Effects")]
    [Tooltip("Particle system for inactive portal")]
    public ParticleSystem inactiveEffect;
    
    [Tooltip("Particle system for active portal")]
    public ParticleSystem activeEffect;
    
    [Tooltip("Light for the portal")]
    public Light2D portalLight;
    
    [Header("Monster Settings")]
    [Tooltip("Prefabs for possible monsters")]
    public GameObject[] monsterPrefabs;
    
    [Tooltip("Spawn point for monsters")]
    public Transform monsterSpawnPoint;
    
    [Header("Audio")]
    [Tooltip("Sound when portal activates")]
    public AudioClip activationSound;
    
    [Tooltip("Sound when monster spawns")]
    public AudioClip monsterSpawnSound;
    
    private bool isActive = false;
    private AudioSource audioSource;
    
    private void Start()
    {
        // Get or add audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Start portal activation cycle
        StartCoroutine(PortalActivationCycle());
    }
    
    private IEnumerator PortalActivationCycle()
    {
        while (true)
        {
            // Wait for activation interval
            yield return new WaitForSeconds(activationInterval);
            
            // Activate portal
            ActivatePortal();
            
            // Wait for activation duration
            yield return new WaitForSeconds(activationDuration);
            
            // Deactivate portal
            DeactivatePortal();
        }
    }
    
    private void ActivatePortal()
    {
        isActive = true;
        
        // Play activation effects
        if (inactiveEffect) inactiveEffect.Stop();
        if (activeEffect) activeEffect.Play();
        
        // Adjust light
        if (portalLight)
        {
            portalLight.intensity = 2f;
            portalLight.color = new Color(0.5f, 0.5f, 1f);
        }
        
        // Play activation sound
        if (activationSound && audioSource)
        {
            audioSource.clip = activationSound;
            audioSource.Play();
        }
        
        // Chance to spawn monster
        if (Random.value < monsterSpawnChance)
        {
            SpawnMonster();
        }
    }
    
    private void DeactivatePortal()
    {
        isActive = false;
        
        // Stop activation effects
        if (activeEffect) activeEffect.Stop();
        if (inactiveEffect) inactiveEffect.Play();
        
        // Adjust light
        if (portalLight)
        {
            portalLight.intensity = 0.5f;
            portalLight.color = new Color(0.3f, 0.3f, 0.5f);
        }
    }
    
    private void SpawnMonster()
    {
        if (monsterPrefabs == null || monsterPrefabs.Length == 0 || monsterSpawnPoint == null)
            return;
        
        // Select random monster
        GameObject monsterPrefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
        
        // Spawn monster
        GameObject monster = Instantiate(monsterPrefab, monsterSpawnPoint.position, Quaternion.identity);
        
        // Play spawn sound
        if (monsterSpawnSound && audioSource)
        {
            audioSource.clip = monsterSpawnSound;
            audioSource.Play();
        }
    }
    
    // Called when player enters the portal trigger
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            // Apply environmental hazard effect to player
            PlayerHazardEffect(other.gameObject);
        }
    }
    
    private void PlayerHazardEffect(GameObject player)
    {
        // Get player movement script
        DiverMovement diverMovement = player.GetComponent<DiverMovement>();
        
        if (diverMovement != null)
        {
            // Apply temporary effect (slowdown, etc.)
            // This would need to be implemented in the DiverMovement script
            // For example: diverMovement.ApplyTemporaryEffect(EffectType.Slowdown, 3f);
        }
    }
} 