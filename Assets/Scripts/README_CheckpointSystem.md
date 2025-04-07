# Checkpoint System Setup Guide

This guide explains how to set up and use the checkpoint system in your game.

## Overview

The checkpoint system allows players to respawn at specific locations when they die, rather than restarting the entire level. The system consists of three main components:

1. **GameManager**: A singleton that tracks the current checkpoint position and handles player respawning.
2. **Checkpoint**: A trigger zone that, when entered by the player, sets the current respawn position.
3. **DiverMovement**: Modified to work with the checkpoint system, allowing the player to respawn at checkpoints.

## Setup Instructions

### 1. Create the GameManager

1. Create an empty GameObject in your scene and name it "GameManager".
2. Add the `GameManager` script to this GameObject.
3. Set the `defaultSpawnPosition` to the initial spawn position of your player.
4. Optionally, set `resetCheckpointsOnReload` to true if you want checkpoints to reset when the scene reloads.

### 2. Create Checkpoints

1. Create a new GameObject for each checkpoint in your level.
2. Add a 2D Collider component (BoxCollider2D, CircleCollider2D, etc.) and set it as a trigger.
3. Add the `Checkpoint` script to the GameObject.
4. Optionally, add visual elements to make the checkpoint visible:
   - Add a SpriteRenderer component and assign a sprite.
   - Add a Light2D component for a glowing effect.
   - Add a ParticleSystem for activation effects.
5. Optionally, add an AudioSource component and assign a sound to play when the checkpoint is activated.

### 3. Configure the Player

1. Select your player GameObject with the DiverMovement script.
2. In the Inspector, find the "Checkpoint Settings" section.
3. Ensure `useCheckpointSystem` is checked.
4. Adjust the `respawnDelay` if needed (default is 3 seconds).

## How It Works

1. When the player enters a checkpoint zone, the checkpoint is activated and its position is saved in the GameManager.
2. If the player dies, they will respawn at the last activated checkpoint after the respawn delay.
3. If no checkpoint has been activated, the player will respawn at the default spawn position.
4. When the player respawns, their health is restored and their state is reset.

## Visual Feedback

The checkpoint system includes visual feedback to help players identify checkpoints:

- Inactive checkpoints are gray.
- Active checkpoints are green.
- When a checkpoint is activated, it plays a particle effect and sound (if configured).

## Advanced Usage

### Multiple Checkpoints

You can place multiple checkpoints throughout your level. The player will always respawn at the last activated checkpoint.

### Conditional Checkpoints

You can modify the `Checkpoint` script to add conditions for activation, such as:
- Requiring the player to collect an item before activating the checkpoint.
- Only allowing activation in certain game states.

### Checkpoint Reset

If you want to reset checkpoints at certain points in the game (e.g., when starting a new level), you can call `GameManager.Instance.ResetCheckpoint()`.

## Troubleshooting

- **Player doesn't respawn at checkpoints**: Make sure the player has the `useCheckpointSystem` option enabled in the DiverMovement script.
- **Checkpoints don't activate**: Ensure the player has the "Player" tag and the checkpoint has a trigger collider.
- **GameManager not found**: Make sure there is a GameObject with the GameManager script in your scene.

## Example Scene Setup

```
Scene Hierarchy:
├── GameManager
│   └── GameManager.cs
├── Player
│   └── DiverMovement.cs
└── Checkpoints
    ├── Checkpoint1
    │   ├── BoxCollider2D (Is Trigger)
    │   ├── SpriteRenderer
    │   ├── Light2D
    │   ├── ParticleSystem
    │   ├── AudioSource
    │   └── Checkpoint.cs
    └── Checkpoint2
        ├── BoxCollider2D (Is Trigger)
        ├── SpriteRenderer
        ├── Light2D
        ├── ParticleSystem
        ├── AudioSource
        └── Checkpoint.cs
``` 