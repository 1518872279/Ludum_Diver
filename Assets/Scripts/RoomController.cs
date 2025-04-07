using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RoomController : MonoBehaviour
{
    [Header("Room Settings")]
    [Tooltip("Type of room")]
    public NodeType roomType = NodeType.Normal;
    
    [Tooltip("Visual indicator for room type")]
    public GameObject normalIndicator;
    public GameObject deadendIndicator;
    public GameObject treasureIndicator;
    public GameObject exitIndicator;
    
    [Header("Room Effects")]
    [Tooltip("Particle system for treasure rooms")]
    public ParticleSystem treasureEffect;
    
    [Tooltip("Particle system for exit rooms")]
    public ParticleSystem exitEffect;
    
    [Tooltip("Light for the room")]
    public Light2D roomLight;
    
    [Header("Room Connections")]
    [Tooltip("Connection points for other rooms")]
    public Transform[] connectionPoints;
    
    private void Start()
    {
        // Set up room based on type
        SetupRoom(roomType);
    }
    
    public void SetupRoom(NodeType type)
    {
        roomType = type;
        
        // Hide all indicators
        if (normalIndicator) normalIndicator.SetActive(false);
        if (deadendIndicator) deadendIndicator.SetActive(false);
        if (treasureIndicator) treasureIndicator.SetActive(false);
        if (exitIndicator) exitIndicator.SetActive(false);
        
        // Show appropriate indicator
        switch (type)
        {
            case NodeType.Normal:
                if (normalIndicator) normalIndicator.SetActive(true);
                break;
            case NodeType.Deadend:
                if (deadendIndicator) deadendIndicator.SetActive(true);
                break;
            case NodeType.Treasure:
                if (treasureIndicator) treasureIndicator.SetActive(true);
                if (treasureEffect) treasureEffect.Play();
                break;
            case NodeType.Exit:
                if (exitIndicator) exitIndicator.SetActive(true);
                if (exitEffect) exitEffect.Play();
                break;
        }
        
        // Adjust room light based on type
        if (roomLight)
        {
            switch (type)
            {
                case NodeType.Normal:
                    roomLight.intensity = 1f;
                    roomLight.color = Color.white;
                    break;
                case NodeType.Deadend:
                    roomLight.intensity = 0.7f;
                    roomLight.color = new Color(1f, 0.5f, 0.5f);
                    break;
                case NodeType.Treasure:
                    roomLight.intensity = 1.2f;
                    roomLight.color = new Color(1f, 0.9f, 0.5f);
                    break;
                case NodeType.Exit:
                    roomLight.intensity = 1.5f;
                    roomLight.color = new Color(0.5f, 1f, 0.5f);
                    break;
            }
        }
    }
    
    // Called when player enters the room
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Notify any listeners that player entered this room
            RoomEntered();
        }
    }
    
    private void RoomEntered()
    {
        // Play room-specific effects
        switch (roomType)
        {
            case NodeType.Treasure:
                // Play treasure room sound
                break;
            case NodeType.Exit:
                // Play exit room sound
                break;
        }
        
        // You could also notify a game manager here
    }
} 