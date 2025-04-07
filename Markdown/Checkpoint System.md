# Checkpoint System Implementation Guide

This document explains how to implement a checkpoint zone system in your game. When the player enters a checkpoint zone, their respawn position is updated. If they reach another checkpoint, the respawn position is reset accordingly.

## 1. Overview

- **Checkpoint Zone:**  
  A designated area (using a trigger collider) that, when entered by the player, sets the current respawn position to that checkpoint.

- **Respawn System:**  
  When the player dies, they are moved to the last activated checkpoint's position. If a new checkpoint is reached, the respawn position updates.

## 2. Components

### 2.1. Checkpoint Zone

- **Game Object Setup:**  
  - Create a checkpoint zone GameObject in your scene.
  - Add a 2D collider (such as a BoxCollider2D) and set it as a trigger.
  - Optionally, add a visual indicator (sprite or particle effect) to signal an active checkpoint.

- **Checkpoint Script:**  
  The checkpoint script will detect when the player enters the zone and update the respawn point.

### 2.2. Game Manager (or Player Controller)

- **Game Manager:**  
  A centralized script (singleton) that holds the current respawn position and provides a function to update it.  
  - When the player dies, the Game Manager will respawn the player at the last saved checkpoint position.
  
## 3. Example Code

### 3.1. Checkpoint Script

Attach this script to each checkpoint zone.

```csharp
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    // Optional: A transform representing the exact respawn position (can be set to the checkpoint zone's position).
    public Transform checkpointSpawnPoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            // Update the player's respawn point
            GameManager.Instance.SetRespawnPoint(checkpointSpawnPoint.position);
            Debug.Log("Checkpoint reached at " + checkpointSpawnPoint.position);

            // Optionally, add visual or audio feedback here
        }
    }
}
