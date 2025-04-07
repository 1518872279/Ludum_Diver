# Enemy Behavior Implementation Guide

This guide outlines the design and implementation details for two enemy types in the deep sea diver game: **Shark** and **Jellyfish**.

---

## 1. Shark Enemy

### Overview

- **Chase and Kill:**  
  The shark detects the player within a specified range (approximately 8–15 meters). When the player is in range, the shark will chase and kill the player upon contact.

- **Patrol Mode:**  
  When the player is out of range, the shark patrols between manually placed patrol nodes using a waypoint system.

- **Customization:**  
  - **Speed:** The shark's movement speed can be adjusted.
  - **Steering Sensitivity:** A higher sensitivity results in faster and more responsive steering.

### Implementation Strategy

1. **Detection:**  
   Use distance checks or a trigger collider to determine when the player is within the chase range. Implement a state machine that toggles between "Patrol" and "Chase" states.

2. **Patrol Behavior:**  
   - Maintain a list of patrol nodes (set manually in the scene).  
   - Move the shark between these nodes using linear interpolation or simple pathfinding.
   - Rotate smoothly toward the next node using steering logic.

3. **Chase Behavior:**  
   - When the player is detected, transition to chase mode.
   - Update the shark's direction and speed toward the player using steering mechanics, factoring in the steering sensitivity.

4. **Player Collision:**  
   On collision with the player, trigger the player’s death sequence.

### Example Code for Shark Behavior

```csharp
using UnityEngine;
using System.Collections.Generic;

public enum SharkState { Patrol, Chase }

public class SharkEnemy : MonoBehaviour
{
    public float chaseRange = 12f; // Average range (8-15 meters)
    public float speed = 5f;
    public float steeringSensitivity = 2f;
    public List<Transform> patrolNodes;
    
    private int currentNodeIndex = 0;
    private SharkState currentState = SharkState.Patrol;
    private Transform player;
    
    void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    void Update() {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Toggle state based on distance
        if (distanceToPlayer <= chaseRange) {
            currentState = SharkState.Chase;
        } else {
            currentState = SharkState.Patrol;
        }
        
        switch (currentState) {
            case SharkState.Patrol:
                Patrol();
                break;
            case SharkState.Chase:
                ChasePlayer();
                break;
        }
    }
    
    void Patrol() {
        if (patrolNodes.Count == 0) return;
        
        Transform targetNode = patrolNodes[currentNodeIndex];
        Vector2 direction = (targetNode.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        // Smoothly rotate towards the patrol node
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), steeringSensitivity * Time.deltaTime);
        
        // Proceed to the next node if close enough
        if (Vector2.Distance(transform.position, targetNode.position) < 1f) {
            currentNodeIndex = (currentNodeIndex + 1) % patrolNodes.Count;
        }
    }
    
    void ChasePlayer() {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);
        
        // Smoothly rotate towards the player
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, 0, angle), steeringSensitivity * Time.deltaTime);
    }
    
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player")) {
            Debug.Log("Player killed by shark!");
            // Insert player death/game over logic here.
        }
    }
}
