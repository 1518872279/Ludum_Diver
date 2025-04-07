# Sonar Mechanic Implementation Guide

This guide explains how to add a sonar mechanic to the diver in the game. The sonar system will:
- Manually release a sonar wave from the diver.
- Emit a wave within a specific range.
- Detect the first collider in its path (it won’t penetrate colliders—if multiple objects are in line, only the closest is detected).
- Spawn a ripple effect at the hit point that lasts for a set duration.
- Have a cooldown period between uses.
- Affect every collider except the player.

## 1. Sonar Mechanic Overview

- **Manual Activation:**  
  The sonar wave is triggered manually (e.g., by a key press) and cannot be spammed due to a cooldown period.

- **Wave Behavior:**  
  The sonar wave is emitted in all directions from the diver’s position.  
  For each direction, a raycast is performed to detect the first collider (ignoring the player).  
  When a collider is hit, a visual ripple effect is instantiated at the hit location.  
  The ripple effect lasts for a specified duration before disappearing.

- **Collision Blocking:**  
  The sonar wave does not penetrate colliders. If two objects are aligned in a single ray’s path, only the first object (the closest) is detected.

## 2. Implementation Strategy

1. **Input and Cooldown:**  
   - Listen for a specific input (for example, the space bar) to trigger the sonar wave.
   - Use a cooldown timer to prevent immediate reuse of the sonar.

2. **Raycasting for Detection:**  
   - Emit a series of raycasts from the diver’s position in a 360° circle.
   - Use a configurable number of rays (e.g., 36 rays for every 10°) to cover all directions.
   - For each ray, if a collider is detected (and it is not the player), instantiate a ripple effect at the impact point.

3. **Ripple Effect:**  
   - Use a prefab for the ripple effect.
   - The ripple effect is spawned at the hit location and destroyed after a set duration.

4. **Excluding the Player:**  
   - Ensure that any collider with the player tag is ignored during the raycast detection.

## 3. Sample Code

Below is an example C# script for Unity that implements the sonar mechanic:

```csharp
using UnityEngine;
using System.Collections;

public class SonarMechanic : MonoBehaviour
{
    [Header("Sonar Settings")]
    public float sonarRange = 20f;         // Maximum distance the sonar wave travels
    public float cooldownTime = 5f;        // Cooldown time between sonar activations
    public int numRays = 36;               // Number of rays cast (360° / 36 = 10° increments)
    public float rippleDuration = 2f;      // Duration the ripple effect lasts
    public GameObject ripplePrefab;        // Prefab for the ripple effect

    private bool canUseSonar = true;
    private Transform diverTransform;

    void Start()
    {
        // Assuming this script is attached to the diver
        diverTransform = transform;
    }

    void Update()
    {
        // Replace KeyCode.Space with your preferred input key
        if (Input.GetKeyDown(KeyCode.Space) && canUseSonar)
        {
            ActivateSonar();
        }
    }

    void ActivateSonar()
    {
        canUseSonar = false;

        // Emit sonar waves in all directions
        for (int i = 0; i < numRays; i++)
        {
            float angle = (360f / numRays) * i;
            Vector2 direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            
            // Cast a ray from the diver's position in the given direction
            RaycastHit2D hit = Physics2D.Raycast(diverTransform.position, direction, sonarRange);
            if (hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                // Spawn a ripple effect at the hit point
                GameObject ripple = Instantiate(ripplePrefab, hit.point, Quaternion.identity);
                Destroy(ripple, rippleDuration);
            }
        }

        // Start the cooldown timer
        StartCoroutine(SonarCooldown());
    }

    IEnumerator SonarCooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        canUseSonar = true;
    }
}
