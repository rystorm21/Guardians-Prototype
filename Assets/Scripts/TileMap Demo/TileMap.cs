using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class TileMap : MonoBehaviour
{
    // Size in number of tiles on our map
    public int size_x;
    public int size_z;
    public float tileSize = 1.0f;

    public Texture2D terrainTiles;
    public int tileResolution;
    
    void Start()
    {
        BuildMesh();
        BuildTexture();
    }

    Color[][] ChopUpTiles() 
    {
        int nummTilesPerRow = terrainTiles.width / tileResolution;
        int numRows = terrainTiles.height / tileResolution;
        Color[][] tiles = new Color[nummTilesPerRow * numRows][];

        for (int y=0; y < numRows; y++) {
            for (int x=0; x < nummTilesPerRow; x++) {
                tiles[y * nummTilesPerRow + x]= terrainTiles.GetPixels(x*tileResolution, y*tileResolution, tileResolution, tileResolution);
            }
        }
        return tiles;
    }

    void BuildTexture() 
    {
        int texWidth = size_x * tileResolution;
        int texHeight = size_z * tileResolution;
        Texture2D texture = new Texture2D(texWidth, texHeight);

        Color[][] tiles = ChopUpTiles();
        
        for (int y = 0; y < size_z; y++) 
        {
            for (int x = 0; x < size_x; x++)
            {
                Color[] p = tiles[Random.Range(0,4)];
                texture.SetPixels(x * tileResolution, y * tileResolution, tileResolution, tileResolution, p);
            }
        }
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.Apply();

        MeshRenderer mesh_renderer = GetComponent<MeshRenderer>();
        mesh_renderer.sharedMaterials[0].mainTexture = texture;
        Debug.Log("Done Texture!");
    }

    // Initialize Map
    public void BuildMesh() 
    {
        int numTiles = size_x * size_z;
        int numTriangles = numTiles * 2;

        int verticesX = size_x + 1;
        int verticesZ = size_z + 1;
        int numVertices = verticesX * verticesZ;

        // Generate the Mesh data
        Vector3[] vertices = new Vector3[numVertices];
        Vector3[] normals = new Vector3[numVertices];
        Vector2[] uv = new Vector2[numVertices];

        int[] triangles = new int[numTriangles * 3];

        int x,z;

        for (z = 0; z < verticesZ; z++) {
            for (x = 0; x < verticesX; x++) {
                vertices[z * verticesX + x] = new Vector3(x * tileSize, 0, z*tileSize);
                normals[z * verticesX + x] = Vector3.up;
                uv[z * verticesX + x] = new Vector2((float)x / size_x, (float)z / size_z);
            }
        }

        for (z = 0; z < size_z; z++) {
            for (x = 0; x < size_x; x++) {
                int squareIndex = z * size_x + x;
                int triOffset = squareIndex * 6;

                triangles[triOffset + 0] = z * verticesX + x +             0;
                triangles[triOffset + 1] = z * verticesX + x + verticesX + 0;
                triangles[triOffset + 2] = z * verticesX + x + verticesX + 1;

                triangles[triOffset + 3] = z * verticesX + x +             0;
                triangles[triOffset + 4] = z * verticesX + x + verticesX + 1; 
                triangles[triOffset + 5] = z * verticesX + x             + 1;
            }
        }

        // Create a new Mesh and populate with the data
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uv;

        // Assign our mesh to our filter/renderer/collider
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
