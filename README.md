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

## Procedural Map Generation

### 1. Set Up the Generator

1. Create an empty GameObject in your scene named "MapGenerator"
2. Add the ProceduralMapGenerator script component
3. Configure the generation settings:
   ```csharp
   Generation Settings:
   - Active Chunks: Number of chunks to keep loaded (default: 5)
   - Chunk Size: Size of each chunk in units (default: 20)
   - Depth Increment: Vertical distance between chunks (default: 10)

   Terrain Settings:
   - Terrain Prefabs: Array of terrain feature prefabs
   - Obstacle Prefabs: Array of obstacle prefabs
   - Collectible Prefabs: Array of collectible prefabs

   Spawn Settings:
   - Obstacle Spawn Chance: Probability of spawning obstacles (0-1)
   - Collectible Spawn Chance: Probability of spawning collectibles (0-1)
   - Spawn Grid Size: Grid divisions for spawn points (default: 4x4)
   ```

### 2. Create Prefabs

1. Terrain Features:
   - Create various rock formations, coral, seaweed
   - Add appropriate colliders
   - Tag as "Terrain"
   - Add to the Terrain Prefabs array

2. Obstacles:
   - Create hazards like spikes, mines, moving creatures
   - Add trigger colliders
   - Tag as "Obstacle"
   - Add to the Obstacle Prefabs array

3. Collectibles:
   - Create collectible items (treasures, power-ups)
   - Add the Collectible script
   - Configure collectible settings:
     ```csharp
     Settings:
     - Point Value: Score awarded
     - Bob Speed: Float animation speed
     - Bob Amount: Float animation distance
     - Rotation Speed: Spin animation speed

     Effects:
     - Collect Effect: Particle system prefab
     - Collect Sound: Audio clip to play
     ```
   - Add trigger collider
   - Tag as "Collectible"
   - Add to the Collectible Prefabs array

### 3. Configure Player

1. Add the "Player" tag to your Diver GameObject
2. Assign the Diver to the Player reference in ProceduralMapGenerator
3. Ensure the Diver has appropriate collision/trigger setup

### 4. Chunk Management

The system automatically manages chunks based on the player's position:
- Creates new chunks as the player descends
- Removes chunks that are too far above
- Maintains a constant number of active chunks
- Uses object pooling for better performance

### 5. Generation Features

1. Terrain Generation:
   - Uses Perlin noise for natural variation
   - Randomly rotates and scales terrain
   - Ensures consistent but varied generation

2. Spawn System:
   - Grid-based spawn points
   - Random offset for natural placement
   - Prevents overlap of obstacles and collectibles
   - Maintains balanced distribution

3. Visual Debugging:
   - Green wireframe shows chunk boundaries
   - Visible in Scene view when selected
   - Helps with level design and testing

## Tips for Map Generation

1. Terrain Design:
   - Create varied terrain prefabs
   - Use different scales and shapes
   - Consider visual themes for depth progression
   - Ensure proper collision setup

2. Obstacle Placement:
   - Balance obstacle density
   - Create patterns that are challenging but fair
   - Consider the diver's movement capabilities
   - Test different spawn chances

3. Collectible Distribution:
   - Place collectibles in risk/reward positions
   - Create interesting paths through obstacles
   - Use collectibles to guide player movement
   - Balance point values with difficulty

4. Performance Optimization:
   - Keep prefabs simple and optimized
   - Use appropriate collider types
   - Consider using object pooling for frequent spawns
   - Monitor frame rate with many objects

## Troubleshooting Generation

1. If chunks aren't generating:
   - Check the Player reference is set
   - Verify Active Chunks value is appropriate
   - Ensure Depth Increment matches your scale
   - Check for errors in the console

2. If objects overlap incorrectly:
   - Adjust the Chunk Size
   - Modify the Spawn Grid Size
   - Check collider setups
   - Verify prefab scales

3. If performance is poor:
   - Reduce Active Chunks
   - Simplify prefab geometry
   - Lower spawn chances
   - Optimize collider usage

4. If generation feels too repetitive:
   - Add more prefab variations
   - Adjust Perlin noise scale
   - Increase spawn point randomization
   - Vary the spawn patterns

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

## Deep Sea Cavern Generator

The Deep Sea Cavern Generator creates a branching cavern system with a single entry point, multiple paths, and various room types. This system provides a more structured and exploration-focused environment compared to the infinite procedural generation.

### 1. Set Up the Cavern Generator

1. Create an empty GameObject in your scene named "CavernGenerator"
2. Add the CavernGenerator script component
3. Configure the generation settings:
   ```csharp
   Generation Settings:
   - Max Depth: Maximum depth of the cavern (default: 5)
   - Max Branches: Maximum number of branches per node (default: 3)
   - Room Spacing: Distance between rooms (default: 10)
   - Player Spawn Point: Transform for player spawning
   
   Room Prefabs:
   - Normal Room Prefab: Prefab for standard rooms
   - Deadend Room Prefab: Prefab for deadend rooms
   - Treasure Room Prefab: Prefab for treasure rooms
   - Exit Room Prefab: Prefab for exit rooms
   
   Obstacle Settings:
   - Obstacle Prefabs: Array of obstacle prefabs
   - Obstacle Spawn Chance: Probability of spawning obstacles (0-1)
   - Max Obstacles Per Room: Maximum obstacles per room
   
   Portal Settings:
   - Portal Prefab: Prefab for portals
   - Portal Spawn Chance: Probability of spawning portals (0-1)
   - Max Portals Per Room: Maximum portals per room
   
   Collectible Settings:
   - Collectible Prefabs: Array of collectible prefabs
   - Collectible Spawn Chance: Probability of spawning collectibles (0-1)
   - Max Collectibles Per Room: Maximum collectibles per room
   ```

### 2. Create Room Prefabs

1. Create prefabs for each room type:
   - Normal Room: Standard passage
   - Deadend Room: Terminal room with no further paths
   - Treasure Room: Room containing valuable items
   - Exit Room: The goal room for the player

2. Add the RoomController script to each room prefab
3. Configure room-specific settings:
   - Visual indicators for room type
   - Particle effects for special rooms
   - Room lighting
   - Connection points for other rooms

### 3. Create Portal Prefab

1. Create a portal prefab with:
   - Visual representation (sprite, particles)
   - Collider2D (set as trigger)
   - Portal script
   - Audio source for effects

2. Configure portal settings:
   - Activation interval and duration
   - Monster spawn chance
   - Visual and audio effects
   - Monster prefabs and spawn point

### 4. Cavern Structure

The cavern is generated using a graph-based approach:
- Single entry point at the top
- Branches extend downward with random horizontal variation
- Each branch can be a normal room, deadend, treasure room, or exit
- At least one exit is guaranteed to exist
- Obstacles, portals, and collectibles are placed within rooms

### 5. Room Types

1. **Normal Rooms:**
   - Standard passage with possible branching
   - Contains basic obstacles and collectibles
   - Neutral lighting

2. **Deadend Rooms:**
   - Terminal room with no further paths
   - Often contains valuable collectibles
   - Slightly darker lighting with red tint

3. **Treasure Rooms:**
   - Contains valuable items and collectibles
   - May have more challenging obstacles
   - Brighter lighting with golden tint
   - Special particle effects

4. **Exit Rooms:**
   - The goal room for the player
   - Usually at the end of a branch
   - Bright lighting with green tint
   - Special particle effects

### 6. Portals

Portals are special objects that:
- Activate periodically
- Have a chance to spawn monsters
- Apply environmental hazards to the player
- Provide visual and audio feedback

### 7. Visual Debugging

The CavernGenerator includes visual debugging tools:
- Node visualization in the Scene view
- Color-coded nodes based on room type
- Connection lines between rooms
- Room boundaries visualization

### 8. Tips for Cavern Design

1. **Room Layout:**
   - Create varied room shapes and sizes
   - Ensure rooms have clear pathways
   - Add visual cues for room types
   - Include connection points for branching

2. **Obstacle Placement:**
   - Place obstacles to create interesting paths
   - Ensure obstacles don't completely block progress
   - Vary obstacle types and patterns
   - Consider the diver's movement capabilities

3. **Portal Design:**
   - Make portals visually distinct
   - Create clear activation/deactivation states
   - Balance monster spawn frequency
   - Design appropriate environmental hazards

4. **Collectible Distribution:**
   - Place valuable collectibles in risk/reward positions
   - Increase collectible value in harder-to-reach rooms
   - Use collectibles to guide player exploration
   - Balance collectible distribution across room types

### 9. Troubleshooting Cavern Generation

1. If rooms aren't connecting properly:
   - Check connection points are properly set up
   - Verify room spacing is appropriate
   - Ensure room prefabs have correct colliders
   - Check for overlapping rooms

2. If generation feels too repetitive:
   - Increase the number of room prefab variations
   - Adjust branching parameters
   - Add more variety to obstacle and collectible placement
   - Modify room type distribution

3. If performance is poor:
   - Reduce maximum depth and branches
   - Simplify room prefabs
   - Optimize particle effects
   - Reduce the number of objects per room

4. If player can't find the exit:
   - Increase the chance of exit rooms
   - Add visual indicators for exit direction
   - Ensure exit rooms are visually distinct
   - Consider adding a minimap or compass

## Enemy System

The game features two types of enemies: Sharks and Jellyfish, each with unique behaviors and attack patterns.

### Shark Enemy Setup
1. Create a new GameObject and name it "Shark"
2. Add the following components:
   - Sprite Renderer
   - Rigidbody2D (set to Dynamic)
   - Circle Collider 2D (set to Trigger)
   - SharkEnemy script
3. Configure the SharkEnemy settings:
   - Detection Settings:
     - Chase Range: Distance at which shark detects player (default: 10)
     - Return Delay: Time before returning to patrol (default: 2)
   - Movement Settings:
     - Base Speed: Normal movement speed (default: 5)
     - Chase Speed Multiplier: Speed increase when chasing (default: 1.5)
     - Max Speed: Maximum movement speed (default: 8)
     - Steering Sensitivity: Turn speed (default: 1.5)
   - Patrol Settings:
     - Patrol Nodes: List of waypoints for patrol path
     - Patrol Wait Time: Time to wait at each node (default: 2)
   - Attack Settings:
     - Damage: Amount of damage dealt (default: 100)
   - Visual Effects:
     - Patrol Effect: Particle system for patrol state
     - Chase Effect: Particle system for chase state
     - Shark Light: Light component for visual feedback
   - Audio:
     - Detection Sound: Played when spotting player
     - Attack Sound: Played when hitting player
     - Ambient Sound: Continuous background sound
4. Set up patrol waypoints:
   - Create empty GameObjects as child objects
   - Position them to form a patrol path
   - Assign them to the patrolNodes list

### Jellyfish Enemy Setup
1. Create a new GameObject and name it "Jellyfish"
2. Add the following components:
   - Sprite Renderer
   - Rigidbody2D (set to Dynamic)
   - Circle Collider 2D (set to Trigger)
   - JellyfishEnemy script
3. Configure the JellyfishEnemy settings:
   - Jellyfish Type:
     - Vertical: Moves up and down
     - Horizontal: Moves left and right
   - Movement Settings:
     - Base Speed: Movement speed (default: 2)
     - Max Speed: Maximum velocity (default: 5)
     - Steering Sensitivity: Turn speed (default: 0.5)
     - Movement Range: Distance from start position (default: 5)
   - Attack Settings:
     - Damage: Amount of damage dealt (default: 100)
   - Visual Effects:
     - Trail Effect: Particle system for movement
     - Jellyfish Light: Light component with pulsing effect
     - Light Pulse Speed: Speed of light pulsing (default: 1)
     - Min/Max Light Intensity: Range of light pulsing
   - Audio:
     - Ambient Sound: Continuous background sound
     - Hit Sound: Played when hitting player
     - Ambient Volume: Volume of ambient sound (0-1)

### Enemy Behavior
- **Sharks**:
  - Patrol between waypoints when player is out of range
  - Chase player when within detection range
  - Deal instant kill damage on contact
  - Visual and audio feedback when detecting player
  - Return to patrol after losing player
- **Jellyfish**:
  - Move in fixed patterns (vertical or horizontal)
  - Deal instant kill damage on contact
  - Visual feedback through pulsing light
  - No active pursuit of player

### Enemy Placement
1. Place enemies in strategic locations:
   - Sharks: Along patrol routes in open areas
   - Vertical Jellyfish: In narrow passages
   - Horizontal Jellyfish: In wide open areas
2. Adjust detection ranges and movement speeds for balanced gameplay
3. Test enemy behavior and adjust settings as needed

### Damage System
1. Player Health:
   - Default max health: 100
   - Enemies deal 100 damage (instant kill)
   - Invulnerability period after taking damage
2. Death Sequence:
   - Visual effects play
   - Audio feedback
   - Scene reloads after delay
3. Enemy Interactions:
   - Sharks: Chase and attack
   - Jellyfish: Passive movement, contact damage
   - Both trigger instant death on contact

## Troubleshooting

### Common Issues
1. **Diver Movement**
   - If movement feels too slow/fast, adjust the force and velocity settings
   - If rotation is too sensitive, reduce the torque force
   - If sinking too quickly, increase the water drag

2. **Camera Issues**
   - If camera movement is jerky, increase the smooth speed
   - If camera gets stuck, check the Y position limits
   - If camera loses track of player, verify the target assignment

3. **Map Generation**
   - If chunks don't generate properly, check the chunk size and active chunks settings
   - If objects overlap, adjust the spawn grid size
   - If generation is too slow, reduce the number of active chunks

4. **Enemy Behavior**
   - If enemies are too aggressive, reduce their detection range or speed
   - If enemies get stuck, check their collider settings
   - If enemies don't detect the player, verify the player tag is set to "Player"
   - If jellyfish movement is erratic, adjust the movement range and speed
   - If shark patrol path is incorrect, check waypoint positions

### Performance Tips
1. Use object pooling for frequently spawned objects
2. Optimize particle effects and reduce their lifetime
3. Use appropriate collider sizes
4. Implement culling for off-screen objects
5. Use the profiler to identify performance bottlenecks 