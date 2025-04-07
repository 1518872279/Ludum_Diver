using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }
    
    [Header("Checkpoint Settings")]
    [Tooltip("Default spawn position if no checkpoint has been reached")]
    public Vector2 defaultSpawnPosition;
    
    [Tooltip("Whether to reset checkpoints when the scene reloads")]
    public bool resetCheckpointsOnReload = false;
    
    // Current checkpoint position
    private Vector2 currentCheckpointPosition;
    private bool hasCheckpoint = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // Initialize with default spawn position
        currentCheckpointPosition = defaultSpawnPosition;
    }
    
    private void Start()
    {
        // If we're reloading the scene and should reset checkpoints
        if (resetCheckpointsOnReload)
        {
            ResetCheckpoint();
        }
    }
    
    // Set a new checkpoint position
    public void SetCheckpoint(Vector2 position)
    {
        currentCheckpointPosition = position;
        hasCheckpoint = true;
        Debug.Log("Checkpoint set at: " + position);
    }
    
    // Get the current checkpoint position
    public Vector2 GetCheckpointPosition()
    {
        return hasCheckpoint ? currentCheckpointPosition : defaultSpawnPosition;
    }
    
    // Check if a checkpoint has been reached
    public bool HasCheckpoint()
    {
        return hasCheckpoint;
    }
    
    // Reset the checkpoint to the default position
    public void ResetCheckpoint()
    {
        currentCheckpointPosition = defaultSpawnPosition;
        hasCheckpoint = false;
        Debug.Log("Checkpoint reset to default position");
    }
    
    // Respawn the player at the current checkpoint
    public void RespawnPlayer(GameObject player)
    {
        if (player != null)
        {
            // Get the DiverMovement component
            DiverMovement diver = player.GetComponent<DiverMovement>();
            
            if (diver != null)
            {
                // Reset the player's position
                player.transform.position = GetCheckpointPosition();
                
                // Reset the player's velocity
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.velocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                }
                
                // Reset the player's health
                diver.ResetPlayer();
                
                Debug.Log("Player respawned at checkpoint: " + GetCheckpointPosition());
            }
            else
            {
                Debug.LogError("Player object does not have a DiverMovement component!");
            }
        }
        else
        {
            Debug.LogError("Player object is null!");
        }
    }
    
    // Reload the current scene
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
} 