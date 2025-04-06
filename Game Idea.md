# Deep Sea Diver Project Implementation Summary

This document outlines the design and implementation plan for a 2D deep sea diver game in Unity using URP. It covers the movement mechanics, visual effects, procedural map generation, and future expansion with a sonar mechanic.

## 1. Overview

- **Game Theme:**  
  Play as a deep sea diver in a dark, mysterious underwater environment with limited visibility.
  
- **Core Mechanics:**  
  - **Movement:** Tank-like controls using left and right mouse buttons.
  - **Lighting:** Diver is equipped with a head light that casts a fan-shaped beam.
  - **Procedural Map:** A continuously descending, procedurally generated underwater map with various obstacles and landscapes.
  - **Future Mechanic:** A sonar feature to detect nearby obstacles, monsters, or treasure.

## 2. Diver Movement & Controls

- **Input Scheme:**
  - **Left Mouse Button:** Controls the diver's left flipper.
    - Applies a forward force.
    - Applies a torque that rotates the diver to the right (clockwise).
  - **Right Mouse Button:** Controls the diver's right flipper.
    - Applies the same forward force.
    - Applies a torque that rotates the diver to the left (counterclockwise).

- **Movement Dynamics:**  
  Alternating button presses cancel out torque for straight movement, while consecutive presses of one button induce turning, similar to driving a tank.

- **Example Code:**
  ```csharp
  using UnityEngine;

  public class DiverMovement : MonoBehaviour
  {
      public float forwardForce = 5f;
      public float torqueForce = 2f;
      private Rigidbody2D rb;

      void Start()
      {
          rb = GetComponent<Rigidbody2D>();
      }

      void Update()
      {
          if (Input.GetMouseButtonDown(0))
          {
              // Left flipper: push forward, turn right
              ApplyForceAndTorque(true);
          }
          else if (Input.GetMouseButtonDown(1))
          {
              // Right flipper: push forward, turn left
              ApplyForceAndTorque(false);
          }
      }

      void ApplyForceAndTorque(bool leftFlipper)
      {
          // Forward impulse in the current facing direction
          Vector2 force = transform.up * forwardForce;
          rb.AddForce(force, ForceMode2D.Impulse);

          // Apply torque based on button pressed
          float torque = leftFlipper ? -torqueForce : torqueForce;
          rb.AddTorque(torque, ForceMode2D.Impulse);
      }
  }
