# Deep Sea Diver - Unity 2D Game

A 2D deep sea diving game where you control a diver using tank-like flipper controls in a dark, mysterious underwater environment.

## Project Setup

1. Create a new Unity project using the 2D URP template
2. Clone this repository or copy the files into your project
3. Ensure you have the following packages installed:
   - Universal RP
   - 2D Sprite
   - 2D Light

## Implementing the Diver Character

### 1. Create the Diver GameObject

1. Create a new empty GameObject in your scene and name it "Diver"
2. Add the following components:
   - Sprite Renderer
   - Rigidbody2D
   - Box Collider 2D or Capsule Collider 2D (depending on your sprite)
   - DiverMovement script

### 2. Configure the Rigidbody2D

1. Select the Diver GameObject
2. In the Rigidbody2D component:
   - Set Body Type to "Dynamic"
   - Uncheck "Gravity Scale" (water physics are handled by the script)
   - Set Linear Drag to 2
   - Set Angular Drag to 2
   - Set Collision Detection to "Continuous"
   - Set Interpolate to "Interpolate"
   - Set Constraints: Freeze Z Position

### 3. Configure the DiverMovement Script

The DiverMovement script has several adjustable parameters:

```csharp
Movement Settings:
- Forward Force: Force applied when pressing a flipper button (default: 5)
- Torque Force: Rotational force for turning (default: 2)
- Max Velocity: Maximum movement speed (default: 10)
- Max Angular Velocity: Maximum rotation speed (default: 180)

Water Physics:
- Water Drag: Drag force in water (default: 2)
- Water Angular Drag: Rotational drag in water (default: 2)
```

Adjust these values to fine-tune the movement feel.

### 4. Add Lighting

1. Create a new GameObject as a child of the Diver
2. Add a 2D Light component
3. Configure the light:
   - Set Light Type to "Spot"
   - Adjust Intensity (recommended: 1-2)
   - Set Inner and Outer Angle for the cone shape
   - Set Distance for the light reach
   - Choose a suitable color (warm white recommended)

### 5. Set Up Camera Following

1. Select the Main Camera in your scene
2. Add the CameraFollow script component
3. Configure the camera settings:
   ```csharp
   Target Settings:
   - Target: Assign your Diver GameObject
   - Offset: Default (0, 0, -10) for 2D games
   
   Follow Settings:
   - Smooth Speed: How quickly camera follows (default: 3)
   - Look Ahead Factor: Camera anticipation of movement (default: 0.5)
   - Max Follow Distance: Snap distance if too far (default: 5)
   
   Depth Settings:
   - Follow Vertical: Enable/disable vertical following
   - Min Y: Lowest point camera can reach (depth limit)
   - Max Y: Highest point camera can reach (surface limit)
   ```
4. Adjust the Camera's properties:
   - Set Projection to "Orthographic"
   - Adjust Size for desired view area (recommended: 5-8)
   - Clear Flags: Solid Color (for underwater effect)
   - Background: Deep blue or black

The camera system features:
- Smooth follow movement using SmoothDamp
- Look-ahead system based on diver's velocity
- Vertical movement constraints for level boundaries
- Maximum follow distance to prevent losing the player
- Editor visualization of boundaries and follow range

## Controls

- Left Mouse Button: Activates left flipper
  - Pushes diver forward
  - Rotates diver clockwise
- Right Mouse Button: Activates right flipper
  - Pushes diver forward
  - Rotates diver counter-clockwise
- Using both buttons together: Moves diver straight forward

## Movement Mechanics

The diver's movement is based on a physics-driven system:

1. Each flipper applies both forward force and rotational torque
2. The forces are applied continuously while the buttons are held
3. Velocity is clamped to prevent excessive speed
4. Water drag provides natural deceleration
5. A slight downward force simulates sinking

## Tips for Testing

1. Start with the default values for forces and drag
2. Test the movement in an empty scene first
3. Adjust the forces and drag values based on your game's needs
4. Use the Scene view gizmo (blue line) to visualize the diver's orientation
5. Consider the camera follow speed when testing movement
6. Use the camera gizmos to visualize follow boundaries
7. Test camera behavior with rapid movement changes

## Next Steps

1. Add underwater particle effects
2. Create obstacles and collectibles
3. Implement the sonar mechanic
4. Add sound effects for movement
5. Create level boundaries
6. Design the procedural map generation system

## Troubleshooting

1. If the diver moves too fast/slow:
   - Adjust the Forward Force value
   - Modify the Max Velocity setting
   - Fine-tune the Water Drag

2. If rotation is too sensitive/sluggish:
   - Adjust the Torque Force value
   - Modify the Max Angular Velocity
   - Fine-tune the Water Angular Drag

3. If the diver sinks too quickly:
   - Reduce the gravity scale in the Start() method
   - Increase the upward component of the forward force

4. If movement feels unresponsive:
   - Check if the Rigidbody2D settings are correct
   - Verify the collider size matches your sprite
   - Ensure no other scripts are interfering with the physics

5. If camera following feels off:
   - Adjust the Smooth Speed for faster/slower following
   - Modify the Look Ahead Factor for better anticipation
   - Check if the Max Follow Distance is appropriate
   - Verify the camera offset is set correctly 