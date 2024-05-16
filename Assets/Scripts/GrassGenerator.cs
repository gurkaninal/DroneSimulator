using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrassGenerator : MonoBehaviour {
    public Mesh grassMesh; // Mesh for grass instances
    public Material grassMaterial; // Material with GPU instancing enabled
    public TerrainMeshGenerator terrainGenerator; // Reference to the terrain generator
    public float spacing = 1.2f; // Spacing between grass instances
    public float randomHeightRange = 0.2f; // Random variation in height

    private Matrix4x4[] instanceMatrices; // Transformation matrices for instancing

    void Start() {
        GenerateGrid();
    }

    void GenerateGrid() {
        // Get the terrain properties
        Vector3 terrainPosition = terrainGenerator.transform.position;
        float terrainWidth = terrainGenerator.meshVariables.terrainWidth;
        float terrainMeshDetail = terrainGenerator.meshVariables.terrainMeshDetail;
        float seaLevel = terrainGenerator.heightmapVariables.waterLevel;
        float maxTerrainHeight = terrainGenerator.meshVariables.height;

        int width = (int) (terrainWidth / spacing) + 1; // Number of rows and columns in the grid

        List<Matrix4x4> instanceList = new();

        int index = 0;
        for (int row = 0; row < width; row++) {
            for (int col = 0; col < width;  col++) {
                // Calculate the position in world coordinates relative to the terrain's position
                float x = col * spacing;
                float z = row * spacing;

                // Convert to the actual world coordinates
                float worldX = terrainPosition.x + x;
                float worldZ = terrainPosition.z + z;

                // Get the corresponding terrain position (between 0 and 1)
                float xNormalized = x / terrainWidth;
                float zNormalized = z / terrainWidth;

                // Get the index of the vertex on the mesh to find the height at that point
                int xIndex = Mathf.FloorToInt(xNormalized * terrainMeshDetail);
                int zIndex = Mathf.FloorToInt(zNormalized * terrainMeshDetail);
                int heightmapIndex = xIndex * (int)(terrainMeshDetail + 1) + zIndex;
                float terrainHeight = terrainGenerator.meshFilter.mesh.vertices[heightmapIndex].y;

                // Include only points above see level (x1.2)
                if (terrainHeight <= seaLevel * 1.5f)
                    continue;
                
                // Randomly discard some instances
                if (GetNormalRandom() < 2f - 3f * terrainHeight / maxTerrainHeight)
                    continue;

                // Find position vector
                float worldY = terrainPosition.y + terrainHeight;
                Vector3 position = new(worldX, worldY, worldZ);

                // Add random scale and rotation
                Vector3 scale = new(1, 1 + Random.Range(0, randomHeightRange), 1);
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);

                instanceList.Add(Matrix4x4.TRS(position, rotation, scale));
                index++;
            }
        }

        instanceMatrices = instanceList.ToArray();
    }

    void Update() {
        if (grassMesh != null && grassMaterial != null && instanceMatrices != null) {
            Graphics.DrawMeshInstanced(grassMesh, 0, grassMaterial, instanceMatrices);
        }
    }

    // Get a random number with a Gaussian distribution
    private static float GetNormalRandom(float mu = 0f, float sigma = 1f) {
        float x1, x2, w, y1;

        do {
            x1 = 2f * (float)Random.Range(0f, 1f) - 1f;
            x2 = 2f * (float)Random.Range(0f, 1f) - 1f;
            w = x1 * x1 + x2 * x2;
        } while (w >= 1f);

        w = Mathf.Sqrt((-2f * Mathf.Log(w)) / w);
        y1 = x1 * w;

        return (y1 * sigma) + mu;
    }
}