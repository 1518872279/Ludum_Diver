using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Points awarded when collected")]
    public int pointValue = 10;

    [Tooltip("Speed of the bobbing motion")]
    public float bobSpeed = 2f;

    [Tooltip("Amount of bobbing motion")]
    public float bobAmount = 0.2f;

    [Tooltip("Speed of rotation")]
    public float rotationSpeed = 90f;

    [Header("Effects")]
    [Tooltip("Particle system to play when collected")]
    public ParticleSystem collectEffect;

    [Tooltip("Sound to play when collected")]
    public AudioClip collectSound;

    private Vector3 startPosition;
    private float bobTime;

    private void Start()
    {
        startPosition = transform.position;
        bobTime = Random.Range(0f, 2f * Mathf.PI); // Random start phase
    }

    private void Update()
    {
        // Update bobbing motion
        bobTime += Time.deltaTime * bobSpeed;
        Vector3 newPosition = startPosition;
        newPosition.y += Mathf.Sin(bobTime) * bobAmount;
        transform.position = newPosition;

        // Update rotation
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        // Spawn collection effect
        if (collectEffect != null)
        {
            ParticleSystem effect = Instantiate(collectEffect, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }

        // Play sound effect
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }

        // Add points or trigger other collection effects here
        // You'll need to implement a scoring system or game manager
        
        // Destroy the collectible
        Destroy(gameObject);
    }
} 