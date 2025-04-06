using UnityEngine;
using System.Collections.Generic;

public class ProceduralMapGenerator : MonoBehaviour
{
    [Header("Generation Settings")]
    [Tooltip("How many chunks to keep loaded at once")]
    public int activeChunks = 5;

    [Tooltip("Size of each chunk in units")]
    public float chunkSize = 20f;

    [Tooltip("Vertical offset between chunks")]
    public float depthIncrement = 10f;

    [Header("Terrain Settings")]
    [Tooltip("Prefabs for terrain features")]
    public GameObject[] terrainPrefabs;

    [Tooltip("Prefabs for obstacles")]
    public GameObject[] obstaclePrefabs;

    [Tooltip("Prefabs for collectibles")]
    public GameObject[] collectiblePrefabs;

    [Header("Spawn Settings")]
    [Range(0f, 1f)]
    [Tooltip("Chance of spawning an obstacle in each spawn point")]
    public float obstacleSpawnChance = 0.3f;

    [Range(0f, 1f)]
    [Tooltip("Chance of spawning a collectible in each spawn point")]
    public float collectibleSpawnChance = 0.2f;

    [Tooltip("Grid size for spawn points within each chunk")]
    public Vector2Int spawnGridSize = new Vector2Int(4, 4);

    [Header("References")]
    [Tooltip("The player transform to check chunk loading")]
    public Transform player;

    private Dictionary<int, GameObject> activeChunkObjects = new Dictionary<int, GameObject>();
    private int currentChunkIndex;
    private float nextChunkDepth;
    private System.Random random;

    private void Start()
    {
        if (player == null)
        {
            Debug.LogError("ProceduralMapGenerator: No player reference assigned!");
            enabled = false;
            return;
        }

        if (terrainPrefabs == null || terrainPrefabs.Length == 0)
        {
            Debug.LogError("ProceduralMapGenerator: No terrain prefabs assigned!");
            enabled = false;
            return;
        }

        // Initialize with a consistent seed for reproducible generation
        random = new System.Random(System.DateTime.Now.Millisecond);
        nextChunkDepth = 0;
        currentChunkIndex = 0;

        // Generate initial chunks
        for (int i = 0; i < activeChunks; i++)
        {
            GenerateChunk(i);
        }
    }

    private void Update()
    {
        if (player == null) return;

        // Check if we need to generate new chunks
        int playerChunkIndex = Mathf.FloorToInt(player.position.y / -depthIncrement);
        
        // Generate new chunks ahead
        while (playerChunkIndex + activeChunks > currentChunkIndex)
        {
            GenerateChunk(currentChunkIndex);
            currentChunkIndex++;
        }

        // Remove chunks that are too far behind
        List<int> chunksToRemove = new List<int>();
        foreach (var chunk in activeChunkObjects)
        {
            if (chunk.Key < playerChunkIndex - 1)
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (int index in chunksToRemove)
        {
            Destroy(activeChunkObjects[index]);
            activeChunkObjects.Remove(index);
        }
    }

    private void GenerateChunk(int index)
    {
        if (activeChunkObjects.ContainsKey(index)) return;

        // Create chunk parent object
        GameObject chunk = new GameObject($"Chunk_{index}");
        chunk.transform.parent = transform;
        chunk.transform.position = new Vector3(0, -index * depthIncrement, 0);
        activeChunkObjects[index] = chunk;

        // Generate terrain
        GenerateTerrain(chunk);

        // Generate spawn points grid
        Vector2 gridCellSize = new Vector2(chunkSize / spawnGridSize.x, chunkSize / spawnGridSize.y);
        
        // Generate obstacles and collectibles
        for (int x = 0; x < spawnGridSize.x; x++)
        {
            for (int y = 0; y < spawnGridSize.y; y++)
            {
                Vector3 spawnPoint = chunk.transform.position + new Vector3(
                    (x + 0.5f) * gridCellSize.x - chunkSize/2,
                    (y + 0.5f) * gridCellSize.y - chunkSize/2,
                    0
                );

                // Add random offset to spawn point
                spawnPoint += new Vector3(
                    Random.Range(-gridCellSize.x * 0.3f, gridCellSize.x * 0.3f),
                    Random.Range(-gridCellSize.y * 0.3f, gridCellSize.y * 0.3f),
                    0
                );

                // Try spawn obstacle
                if (Random.value < obstacleSpawnChance && obstaclePrefabs != null && obstaclePrefabs.Length > 0)
                {
                    GameObject obstacle = Instantiate(
                        obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)],
                        spawnPoint,
                        Quaternion.Euler(0, 0, Random.Range(0f, 360f)),
                        chunk.transform
                    );
                }
                // If no obstacle, try spawn collectible
                else if (Random.value < collectibleSpawnChance && collectiblePrefabs != null && collectiblePrefabs.Length > 0)
                {
                    GameObject collectible = Instantiate(
                        collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)],
                        spawnPoint,
                        Quaternion.identity,
                        chunk.transform
                    );
                }
            }
        }
    }

    private void GenerateTerrain(GameObject chunk)
    {
        if (terrainPrefabs == null || terrainPrefabs.Length == 0) return;

        // Generate perlin noise for terrain variation
        float noiseScale = 0.5f;
        float noiseValue = Mathf.PerlinNoise(
            chunk.transform.position.x * noiseScale,
            chunk.transform.position.y * noiseScale
        );

        // Select terrain prefab based on noise
        int terrainIndex = Mathf.FloorToInt(noiseValue * terrainPrefabs.Length);
        terrainIndex = Mathf.Clamp(terrainIndex, 0, terrainPrefabs.Length - 1);

        // Instantiate terrain
        GameObject terrain = Instantiate(
            terrainPrefabs[terrainIndex],
            chunk.transform.position,
            Quaternion.identity,
            chunk.transform
        );

        // Add some random rotation and scale variation
        terrain.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        float scaleVariation = Random.Range(0.8f, 1.2f);
        terrain.transform.localScale *= scaleVariation;
    }

    // Helper method to visualize chunk boundaries in the editor
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        foreach (var chunk in activeChunkObjects)
        {
            Gizmos.color = Color.green;
            Vector3 center = chunk.Value.transform.position;
            Vector3 size = new Vector3(chunkSize, chunkSize, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
} 