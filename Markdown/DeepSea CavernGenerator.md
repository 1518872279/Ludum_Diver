# Deep Sea Cavern Procedural Map Generator

This document describes an implementation plan for a deep sea cavern map generator in Unity. The generator creates a single-entry cavern from above (where the player spawns) and expands into multiple branching paths that randomly lead to deadends, treasure rooms, or an exit. Obstacles are randomly placed in a way that they do not completely block the passage, and portals are added as placeholders for future procedural monsters and environmental hazards.

## 1. Map Overview

- **Single Entry:**  
  The cavern starts with one entrance from above where the player spawns.

- **Branching Layout:**  
  The cavern branches into multiple corridors. Each branch may lead to:
  - **Exit:** One branch must be the exit of the map.
  - **Deadend:** A corridor that ends without further progress.
  - **Treasure:** A branch containing a treasure area.
  
- **Obstacles:**  
  Random obstacles are placed in each room/corridor. Their placement is controlled so that the path is never entirely blocked.

- **Portals:**  
  Portals are placed at random locations within the map for future integration of procedurally generated monsters or environmental hazards.

## 2. Implementation Strategy

### 2.1 Graph-Based Generation

- **Node Representation:**  
  Each room or corridor is represented by a node that contains its type:
  - **Normal:** A regular passage.
  - **Deadend:** A terminating branch.
  - **Treasure:** Contains treasure.
  - **Exit:** The exit branch of the map.

- **Branching Rules:**  
  Starting from the single entry node, recursively generate child nodes (branches) with random types. Use a maximum depth to control map size.

- **Guaranteeing an Exit:**  
  Ensure that at least one branch is marked as the exit. This can be done by either forcing one leaf node to become the exit or by randomly assigning an exit among deeper nodes.

### 2.2 Obstacle and Portal Placement

- **Obstacle Placement:**  
  In each generated room, place obstacles randomly but leave a clear pathway for the player. This might involve positioning obstacles along the sides or away from the central passage.

- **Portal Placement:**  
  Randomly place portals in rooms based on a given probability. These portals will later serve as triggers for spawning monsters or activating hazards.

## 3. Example Code Implementation

Below is an example implementation using C# in Unity.

### 3.1 Cavern Node and Map Generator

```csharp
using UnityEngine;
using System.Collections.Generic;

public enum NodeType {
    Normal,
    Deadend,
    Treasure,
    Exit
}

public class CavernNode {
    public NodeType nodeType;
    public Vector2 position;
    public List<CavernNode> branches;

    public CavernNode(Vector2 pos, NodeType type = NodeType.Normal) {
        position = pos;
        nodeType = type;
        branches = new List<CavernNode>();
    }
}

public class CavernMapGenerator : MonoBehaviour {
    public GameObject roomPrefab; // Prefab representing a room/corridor
    public int maxDepth = 5;
    public int maxBranches = 3;
    public float roomSpacing = 10f;
    public Transform playerSpawnPoint;

    private CavernNode rootNode;

    void Start() {
        // Create the single entry node where the player spawns.
        rootNode = new CavernNode(new Vector2(0, 0));
        GenerateBranches(rootNode, 0);
        EnsureExitExists(rootNode);
        InstantiateCavern(rootNode);
    }

    void GenerateBranches(CavernNode node, int depth) {
        if (depth >= maxDepth) return;

        int branchCount = Random.Range(1, maxBranches + 1);
        for (int i = 0; i < branchCount; i++) {
            // Calculate branch position below the current node with slight horizontal variation.
            Vector2 branchPos = node.position + new Vector2(Random.Range(-roomSpacing, roomSpacing), -roomSpacing);
            // Randomly assign node types (Normal, Deadend, Treasure)
            NodeType type = NodeType.Normal;
            float chance = Random.value;
            if (chance < 0.2f)
                type = NodeType.Treasure;
            else if (chance < 0.4f)
                type = NodeType.Deadend;
            
            CavernNode child = new CavernNode(branchPos, type);
            node.branches.Add(child);
            GenerateBranches(child, depth + 1);
        }
    }

    void EnsureExitExists(CavernNode node) {
        // Recursively ensure at least one node in the graph is marked as the exit.
        if (node.branches.Count == 0) {
            // Leaf node candidate becomes the exit.
            node.nodeType = NodeType.Exit;
        } else {
            bool exitFound = false;
            foreach (var branch in node.branches) {
                if (branch.nodeType == NodeType.Exit) {
                    exitFound = true;
                    break;
                }
            }
            if (!exitFound && Random.value < 0.5f) {
                // Force one branch to be the exit.
                int index = Random.Range(0, node.branches.Count);
                node.branches[index].nodeType = NodeType.Exit;
            }
            foreach (var branch in node.branches) {
                EnsureExitExists(branch);
            }
        }
    }

    void InstantiateCavern(CavernNode node) {
        // Instantiate a room at the node's position.
        GameObject room = Instantiate(roomPrefab, new Vector3(node.position.x, node.position.y, 0), Quaternion.identity);
        // Setup the room content based on node type.
        RoomController roomController = room.GetComponent<RoomController>();
        if (roomController != null) {
            roomController.SetupRoom(node.nodeType);
        }
        // Recursively instantiate child nodes.
        foreach (var branch in node.branches) {
            InstantiateCavern(branch);
        }
    }
}
