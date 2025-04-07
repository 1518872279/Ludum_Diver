# Trap Implementation Guide for Deep Sea Diver

This document describes the design and sample implementation for three types of traps in the game: **Whirlpool**, **Undercurrent**, and **Sharp Rocks**.

---

## 1. Whirlpool Trap

### Behavior
- **Force Generation:**  
  The trap generates a force to pull players toward its center.
  
- **Circular Movement:**  
  Once the player is dragged into the trap, a tangential (perpendicular) force is applied so that the player is forced into a circular, spinning movement around the center.

### Implementation Strategy
- Use a trigger collider to detect when the player is within the Whirlpool.
- In `OnTriggerStay2D`, calculate the vector from the player to the trap's center.
- Apply a force along that vector (drag force) and add an additional perpendicular force (circular force).

### Sample Code
```csharp
using UnityEngine;

public class WhirlpoolTrap : MonoBehaviour
{
    public float dragForce = 10f;
    public float circularForce = 5f;

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
            if(rb != null)
            {
                // Calculate the direction to the trap center.
                Vector2 directionToCenter = ((Vector2)transform.position - rb.position).normalized;
                // Apply drag force to pull the player towards the center.
                rb.AddForce(directionToCenter * dragForce * Time.deltaTime, ForceMode2D.Force);
                
                // Calculate a perpendicular vector to create circular motion.
                Vector2 perpendicular = new Vector2(-directionToCenter.y, directionToCenter.x);
                rb.AddForce(perpendicular * circularForce * Time.deltaTime, ForceMode2D.Force);
            }
        }
    }
}
