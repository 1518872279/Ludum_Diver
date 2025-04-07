using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
    public GameObject roomObject;

    public CavernNode(Vector2 pos, NodeType type = NodeType.Normal) {
        position = pos;
        nodeType = type;
        branches = new List<CavernNode>();
    }
}

public class CavernGenerator : MonoBehaviour {
    [Header("Generation Settings")]
    [Tooltip("Maximum depth of the cavern")]
    public int maxDepth = 5;
    
    [Tooltip("Maximum number of branches per node")]
    public int maxBranches = 3;
    
    [Tooltip("Distance between rooms")]
    public float roomSpacing = 10f;
    
    [Tooltip("Player spawn point")]
    public Transform playerSpawnPoint;

    [Header("Room Prefabs")]
    [Tooltip("Prefab for normal rooms")]
    public GameObject normalRoomPrefab;
    
    [Tooltip("Prefab for deadend rooms")]
    public GameObject deadendRoomPrefab;
    
    [Tooltip("Prefab for treasure rooms")]
    public GameObject treasureRoomPrefab;
    
    [Tooltip("Prefab for exit rooms")]
    public GameObject exitRoomPrefab;

    [Header("Obstacle Settings")]
    [Tooltip("Prefabs for obstacles")]
    public GameObject[] obstaclePrefabs;
    
    [Tooltip("Chance of spawning an obstacle in each room")]
    [Range(0f, 1f)]
    public float obstacleSpawnChance = 0.3f;
    
    [Tooltip("Maximum number of obstacles per room")]
    public int maxObstaclesPerRoom = 3;

    [Header("Portal Settings")]
    [Tooltip("Prefab for portals")]
    public GameObject portalPrefab;
    
    [Tooltip("Chance of spawning a portal in each room")]
    [Range(0f, 1f)]
    public float portalSpawnChance = 0.2f;
    
    [Tooltip("Maximum number of portals per room")]
    public int maxPortalsPerRoom = 1;

    [Header("Collectible Settings")]
    [Tooltip("Prefabs for collectibles")]
    public GameObject[] collectiblePrefabs;
    
    [Tooltip("Chance of spawning a collectible in each room")]
    [Range(0f, 1f)]
    public float collectibleSpawnChance = 0.4f;
    
    [Tooltip("Maximum number of collectibles per room")]
    public int maxCollectiblesPerRoom = 2;

    [Header("Editor Settings")]
    [Tooltip("Seed for random generation (0 = random)")]
    public int randomSeed = 0;
    
    [Tooltip("Parent object for generated rooms")]
    public Transform roomsParent;

    private CavernNode rootNode;
    private List<CavernNode> allNodes = new List<CavernNode>();
    private System.Random random;
    private bool isGenerated = false;

    void Start() {
        if (!isGenerated) {
            GenerateCavern();
        }
    }
    
    public void GenerateCavern() {
        // Clear existing cavern if any
        ClearCavern();
        
        // Initialize random with seed
        if (randomSeed != 0) {
            random = new System.Random(randomSeed);
            Random.InitState(randomSeed);
        } else {
            random = new System.Random(System.DateTime.Now.Millisecond);
        }
        
        // Create the single entry node where the player spawns
        rootNode = new CavernNode(new Vector2(0, 0));
        allNodes.Add(rootNode);
        
        // Generate the cavern structure
        GenerateBranches(rootNode, 0);
        
        // Ensure at least one exit exists
        EnsureExitExists(rootNode);
        
        // Instantiate the cavern
        InstantiateCavern(rootNode);
        
        // Place player at spawn point
        if (playerSpawnPoint != null) {
            playerSpawnPoint.position = new Vector3(rootNode.position.x, rootNode.position.y, 0);
        }
        
        isGenerated = true;
    }
    
    public void ClearCavern() {
        // Destroy all existing rooms
        if (roomsParent != null) {
            #if UNITY_EDITOR
            if (!Application.isPlaying) {
                // In editor mode, use DestroyImmediate
                while (roomsParent.childCount > 0) {
                    DestroyImmediate(roomsParent.GetChild(0).gameObject);
                }
            } else {
                // In play mode, use Destroy
                foreach (Transform child in roomsParent) {
                    Destroy(child.gameObject);
                }
            }
            #else
            foreach (Transform child in roomsParent) {
                Destroy(child.gameObject);
            }
            #endif
        } else {
            // If no parent specified, find and destroy all rooms
            GameObject[] rooms = GameObject.FindGameObjectsWithTag("Room");
            foreach (GameObject room in rooms) {
                #if UNITY_EDITOR
                if (!Application.isPlaying) {
                    DestroyImmediate(room);
                } else {
                    Destroy(room);
                }
                #else
                Destroy(room);
                #endif
            }
        }
        
        // Reset state
        rootNode = null;
        allNodes.Clear();
        isGenerated = false;
    }

    void GenerateBranches(CavernNode node, int depth) {
        if (depth >= maxDepth) return;

        // Determine number of branches for this node
        int branchCount = Random.Range(1, maxBranches + 1);
        
        for (int i = 0; i < branchCount; i++) {
            // Calculate branch position below the current node with slight horizontal variation
            Vector2 branchPos = node.position + new Vector2(
                Random.Range(-roomSpacing, roomSpacing), 
                -roomSpacing
            );
            
            // Randomly assign node types (Normal, Deadend, Treasure)
            NodeType type = NodeType.Normal;
            float chance = Random.value;
            
            if (chance < 0.2f)
                type = NodeType.Treasure;
            else if (chance < 0.4f)
                type = NodeType.Deadend;
            
            CavernNode child = new CavernNode(branchPos, type);
            node.branches.Add(child);
            allNodes.Add(child);
            
            // Recursively generate branches for this child
            GenerateBranches(child, depth + 1);
        }
    }

    void EnsureExitExists(CavernNode node) {
        // Check if this node has any branches
        if (node.branches.Count == 0) {
            // Leaf node candidate becomes the exit
            node.nodeType = NodeType.Exit;
            return;
        }
        
        // Check if any branch is already an exit
        bool exitFound = false;
        foreach (var branch in node.branches) {
            if (branch.nodeType == NodeType.Exit) {
                exitFound = true;
                break;
            }
        }
        
        // If no exit found and random chance, force one branch to be the exit
        if (!exitFound && Random.value < 0.5f) {
            int index = Random.Range(0, node.branches.Count);
            node.branches[index].nodeType = NodeType.Exit;
        }
        
        // Recursively check all branches
        foreach (var branch in node.branches) {
            EnsureExitExists(branch);
        }
    }

    void InstantiateCavern(CavernNode node) {
        // Select the appropriate prefab based on node type
        GameObject prefab = normalRoomPrefab;
        switch (node.nodeType) {
            case NodeType.Deadend:
                prefab = deadendRoomPrefab;
                break;
            case NodeType.Treasure:
                prefab = treasureRoomPrefab;
                break;
            case NodeType.Exit:
                prefab = exitRoomPrefab;
                break;
        }
        
        // Instantiate the room
        GameObject room;
        #if UNITY_EDITOR
        if (!Application.isPlaying) {
            // In editor mode, use PrefabUtility
            room = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        } else {
            // In play mode, use Instantiate
            room = Instantiate(prefab);
        }
        #else
        room = Instantiate(prefab);
        #endif
        
        // Position the room
        room.transform.position = new Vector3(node.position.x, node.position.y, 0);
        
        // Set parent if specified
        if (roomsParent != null) {
            room.transform.SetParent(roomsParent);
        }
        
        // Tag the room for easy finding
        room.tag = "Room";
        
        // Store reference to the room
        node.roomObject = room;
        
        // Add obstacles, portals, and collectibles
        PopulateRoom(node);
        
        // Recursively instantiate child nodes
        foreach (var branch in node.branches) {
            InstantiateCavern(branch);
        }
    }

    void PopulateRoom(CavernNode node) {
        if (node.roomObject == null) return;
        
        // Get the room's bounds for placing objects
        Bounds roomBounds = GetRoomBounds(node.roomObject);
        
        // Place obstacles
        int obstacleCount = Random.Range(0, maxObstaclesPerRoom + 1);
        for (int i = 0; i < obstacleCount; i++) {
            if (Random.value < obstacleSpawnChance && obstaclePrefabs != null && obstaclePrefabs.Length > 0) {
                PlaceObjectInRoom(node.roomObject, obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)], roomBounds);
            }
        }
        
        // Portals are temporarily disabled
        // if (Random.value < portalSpawnChance && portalPrefab != null) {
        //     PlaceObjectInRoom(node.roomObject, portalPrefab, roomBounds);
        // }
        
        // Place collectibles
        int collectibleCount = Random.Range(0, maxCollectiblesPerRoom + 1);
        for (int i = 0; i < collectibleCount; i++) {
            if (Random.value < collectibleSpawnChance && collectiblePrefabs != null && collectiblePrefabs.Length > 0) {
                PlaceObjectInRoom(node.roomObject, collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)], roomBounds);
            }
        }
    }

    Bounds GetRoomBounds(GameObject room) {
        // Get the room's collider bounds or create a default bounds
        Collider2D collider = room.GetComponent<Collider2D>();
        if (collider != null) {
            return collider.bounds;
        }
        
        // Default bounds if no collider found
        return new Bounds(room.transform.position, new Vector3(roomSpacing * 0.8f, roomSpacing * 0.8f, 1f));
    }

    void PlaceObjectInRoom(GameObject room, GameObject prefab, Bounds roomBounds) {
        // Calculate a random position within the room bounds
        Vector3 position = new Vector3(
            Random.Range(roomBounds.min.x + 1f, roomBounds.max.x - 1f),
            Random.Range(roomBounds.min.y + 1f, roomBounds.max.y - 1f),
            0
        );
        
        // Instantiate the object
        GameObject obj;
        #if UNITY_EDITOR
        if (!Application.isPlaying) {
            // In editor mode, use PrefabUtility
            obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        } else {
            // In play mode, use Instantiate
            obj = Instantiate(prefab);
        }
        #else
        obj = Instantiate(prefab);
        #endif
        
        // Position and parent the object
        obj.transform.position = position;
        obj.transform.SetParent(room.transform);
        
        // Add some random rotation
        obj.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
    }

    // Helper method to visualize the cavern structure in the editor
    void OnDrawGizmos() {
        if (rootNode == null) return;
        
        DrawNodeGizmos(rootNode);
    }

    void DrawNodeGizmos(CavernNode node) {
        // Draw the node
        Gizmos.color = GetNodeColor(node.nodeType);
        Gizmos.DrawWireSphere(new Vector3(node.position.x, node.position.y, 0), 1f);
        
        // Draw connections to branches
        foreach (var branch in node.branches) {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(
                new Vector3(node.position.x, node.position.y, 0),
                new Vector3(branch.position.x, branch.position.y, 0)
            );
            
            // Recursively draw branch nodes
            DrawNodeGizmos(branch);
        }
    }

    Color GetNodeColor(NodeType type) {
        switch (type) {
            case NodeType.Normal:
                return Color.white;
            case NodeType.Deadend:
                return Color.red;
            case NodeType.Treasure:
                return Color.yellow;
            case NodeType.Exit:
                return Color.green;
            default:
                return Color.gray;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CavernGenerator))]
public class CavernGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Get the target
        CavernGenerator generator = (CavernGenerator)target;
        
        // Add a space
        EditorGUILayout.Space();
        
        // Add buttons for editor functionality
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Generate Cavern"))
        {
            generator.GenerateCavern();
        }
        
        if (GUILayout.Button("Clear Cavern"))
        {
            generator.ClearCavern();
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Add a note about editor mode
        EditorGUILayout.HelpBox("Use these buttons to generate or clear the cavern in Editor Mode.", MessageType.Info);
    }
}
#endif 