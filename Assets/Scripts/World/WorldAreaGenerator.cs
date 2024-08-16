using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NoiseData
{
    public float baseValue;
    public float noiseScale;
    public float noiseStrength;

    public float Sample(float x, float y)
    {
        return baseValue + noiseStrength * Mathf.PerlinNoise(x * noiseScale, y * noiseScale);
    }
}

[Serializable]
public class BiomeGenerationData
{
    public NoiseData groundHeight;
    public NoiseData waterHeight;
    public float waterEdgeDistance;
    public NoiseData waterEdgeWobble;
    public Vector3 waterHeightNoiseChange;
    public Material GroundMaterial;
    public Material GrassMaterial;
    public Material WaterMaterial;
}

public class WorldAreaGenerator : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] public BiomeGenerationData BiomeData;
    [SerializeField] public Vector2Int AreaSize;
    [SerializeField] public bool ToGenerateGrass = true;
    [Range(1, 5)]
    [SerializeField] public int SubdivisionCount = 1;

    [ContextMenu("Generate")]
    public void Generate()
    {
        Clear();
        GenerateGroundHeightmap();
        GenerateGround();
        GenerateWater();
    }

    private int subdivSizeX, subdivSizeY;
    private float subdivScaleX, subdivScaleY;
    private float[][] heightmap;
    private GameObject groundGO;
    private GameObject grassGO;
    private GameObject waterGO;

    private void Clear()
    {
        // Delete all children
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // Clear all data
        waterGO = null;
        grassGO = null;
    }

    private void GenerateGroundHeightmap()
    {
        // Setup variables
        subdivSizeX = AreaSize.x * SubdivisionCount;
        subdivSizeY = AreaSize.y * SubdivisionCount;
        subdivScaleX = 1.0f / SubdivisionCount;
        subdivScaleY = 1.0f / SubdivisionCount;

        // Generate height map
        heightmap = new float[subdivSizeX + 1][];
        for (int x = 0; x <= subdivSizeX; x++)
        {
            heightmap[x] = new float[subdivSizeY + 1];
            for (int y = 0; y <= subdivSizeY; y++)
            {
                float nx = (float)x * subdivScaleX;
                float ny = (float)y * subdivScaleY;
                heightmap[x][y] = BiomeData.groundHeight.Sample(nx, ny);
            }
        }
    }

    private void GenerateGround()
    {
        // Setup ground object
        groundGO = new GameObject("Ground");
        groundGO.transform.parent = transform;
        groundGO.transform.localPosition = Vector3.zero;
        MeshFilter groundMF = groundGO.AddComponent<MeshFilter>();
        MeshRenderer groundMR = groundGO.AddComponent<MeshRenderer>();
        List<Vector3> groundVerts = new List<Vector3>();
        List<int> groundTris = new List<int>();

        // Setup grass object
        grassGO = new GameObject("Grass");
        grassGO.transform.parent = transform;
        grassGO.transform.localPosition = Vector3.zero;
        MeshFilter grassMF = grassGO.AddComponent<MeshFilter>();
        MeshRenderer grassMR = grassGO.AddComponent<MeshRenderer>();
        List<Vector3> grassVerts = new List<Vector3>();
        List<int> grassTris = new List<int>();
        List<Color> grassColors = new List<Color>();

        var addQuad = new Action<List<int>, int, int, int, int>((tris, a, b, c, d) =>
        {
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
            tris.Add(c);
            tris.Add(b);
            tris.Add(d);
        });

        // Generate vertices and triangles for bottom
        groundVerts.Add(new Vector3(0, 0, 0));
        groundVerts.Add(new Vector3(AreaSize.x, 0, 0));
        groundVerts.Add(new Vector3(0, 0, AreaSize.y));
        groundVerts.Add(new Vector3(AreaSize.x, 0, AreaSize.y));
        addQuad(groundTris, 0, 1, 2, 3);

        // Generate all surface vertices
        for (int x = 0; x <= subdivSizeX; x++)
        {
            for (int y = 0; y <= subdivSizeY; y++)
            {
                groundVerts.Add(new Vector3(x * subdivScaleX, heightmap[x][y], y * subdivScaleY));
                grassVerts.Add(new Vector3(x * subdivScaleX, heightmap[x][y] + 0.005f, y * subdivScaleY));

                // Setup grass colours based on sea level height
                float waterLevel = BiomeData.waterHeight.baseValue + BiomeData.waterHeight.noiseStrength * 0.5f;
                float edgeDistance = (heightmap[x][y] - waterLevel - BiomeData.waterEdgeWobble.Sample(x * subdivScaleX, y * subdivScaleY));
                float r = Mathf.Clamp01(edgeDistance / BiomeData.waterEdgeDistance);
                grassColors.Add(new Color(1, 1, 1, r));
            }
        }

        // Generate all surface triangles
        for (int x = 0; x < subdivSizeX; x++)
        {
            for (int y = 0; y < subdivSizeY; y++)
            {
                int topLeft = 4 + x * (subdivSizeY + 1) + y + 1;
                int topRight = 4 + (x + 1) * (subdivSizeY + 1) + y + 1;
                int bottomLeft = 4 + x * (subdivSizeY + 1) + y;
                int bottomRight = 4 + (x + 1) * (subdivSizeY + 1) + y;
                addQuad(groundTris, topLeft, topRight, bottomLeft, bottomRight);
                addQuad(grassTris, topLeft - 4, topRight - 4, bottomLeft - 4, bottomRight - 4);
            }
        }

        // Generate ground edge geometry
        for (int x = 0; x < subdivSizeX; x++)
        {
            // Setup south edge ground geometry
            int southTopLeft = 4 + x * (subdivSizeY + 1);
            int southTopRight = 4 + (x + 1) * (subdivSizeY + 1);
            int southBottomIndex = groundVerts.Count;
            groundVerts.Add(new Vector3((x) * subdivScaleX, 0, 0));
            groundVerts.Add(new Vector3((x + 1) * subdivScaleX, 0, 0));
            addQuad(groundTris, southTopLeft, southTopRight, southBottomIndex, southBottomIndex + 1);

            // Setup north edge ground geometry
            int northTopLeft = 4 + (x + 2) * (subdivSizeY + 1) - 1;
            int northTopRight = 4 + (x + 1) * (subdivSizeY + 1) - 1;
            int northIndex = groundVerts.Count;
            groundVerts.Add(new Vector3((x + 1) * subdivScaleX, 0, AreaSize.y));
            groundVerts.Add(new Vector3((x) * subdivScaleX, 0, AreaSize.y));
            addQuad(groundTris, northTopLeft, northTopRight, northIndex, northIndex + 1);
        }
        for (int y = 0; y < subdivSizeY; y++)
        {
            // Setup west edge ground geometry
            int westTopLeft = 4 + y + 1;
            int westTopRight = 4 + y;
            int westIndex = groundVerts.Count;
            groundVerts.Add(new Vector3(0, 0, (y + 1) * subdivScaleY));
            groundVerts.Add(new Vector3(0, 0, (y) * subdivScaleY));
            addQuad(groundTris, westTopLeft, westTopRight, westIndex, westIndex + 1);

            // Setup east edge geometry
            int eastTopLeft = 4 + (subdivSizeX) * (subdivSizeY + 1) + y;
            int eastTopRight = 4 + (subdivSizeX) * (subdivSizeY + 1) + y + 1;
            int eastIndex = groundVerts.Count;
            groundVerts.Add(new Vector3(AreaSize.x, 0, (y) * subdivScaleY));
            groundVerts.Add(new Vector3(AreaSize.x, 0, (y + 1) * subdivScaleY));
            addQuad(groundTris, eastTopLeft, eastTopRight, eastIndex, eastIndex + 1);
        }

        // Assign all ground data
        Mesh mesh = new Mesh();
        mesh.vertices = groundVerts.ToArray();
        mesh.triangles = groundTris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        groundMF.mesh = mesh;
        groundMR.material = BiomeData.GroundMaterial;

        // Assign all grass data
        Mesh grassMesh = new Mesh();
        grassMesh.vertices = grassVerts.ToArray();
        grassMesh.triangles = grassTris.ToArray();
        grassMesh.colors = grassColors.ToArray();
        grassMesh.RecalculateNormals();
        grassMesh.RecalculateBounds();
        grassMF.mesh = grassMesh;
        grassMR.material = BiomeData.GrassMaterial;
    }

    private void GenerateWater()
    {
        // Setup water object
        waterGO = new GameObject("Water");
        waterGO.transform.parent = transform;
        waterGO.transform.localPosition = Vector3.zero;
        MeshFilter groundMF = waterGO.AddComponent<MeshFilter>();
        MeshRenderer groundMR = waterGO.AddComponent<MeshRenderer>();
        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        var addQuad = new Action<List<int>, int, int, int, int>((tris, a, b, c, d) =>
        {
            tris.Add(a);
            tris.Add(b);
            tris.Add(c);
            tris.Add(c);
            tris.Add(b);
            tris.Add(d);
        });

        // Generate vertices and triangles for bottom
        verts.Add(new Vector3(0, 0, 0));
        verts.Add(new Vector3(AreaSize.x, 0, 0));
        verts.Add(new Vector3(0, 0, AreaSize.y));
        verts.Add(new Vector3(AreaSize.x, 0, AreaSize.y));
        addQuad(tris, 0, 1, 2, 3);

        // Generate all surface vertices
        for (int x = 0; x <= subdivSizeX; x++)
        {
            for (int y = 0; y <= subdivSizeY; y++)
            {
                float h = BiomeData.waterHeight.Sample(x * subdivScaleX, y * subdivScaleY);
                verts.Add(new Vector3(x * subdivScaleX, h, y * subdivScaleY));
            }
        }

        // Generate all surface triangles
        for (int x = 0; x < subdivSizeX; x++)
        {
            for (int y = 0; y < subdivSizeY; y++)
            {
                int topLeft = 4 + x * (subdivSizeY + 1) + y + 1;
                int topRight = 4 + (x + 1) * (subdivSizeY + 1) + y + 1;
                int bottomLeft = 4 + x * (subdivSizeY + 1) + y;
                int bottomRight = 4 + (x + 1) * (subdivSizeY + 1) + y;
                addQuad(tris, topLeft, topRight, bottomLeft, bottomRight);
            }
        }

        // Generate edge geometry
        for (int x = 0; x < subdivSizeX; x++)
        {
            // Setup south edge geometry
            int southTopLeft = 4 + x * (subdivSizeY + 1);
            int southTopRight = 4 + (x + 1) * (subdivSizeY + 1);
            int southBottomIndex = verts.Count;
            verts.Add(new Vector3((x) * subdivScaleX, 0, 0));
            verts.Add(new Vector3((x + 1) * subdivScaleX, 0, 0));
            addQuad(tris, southTopLeft, southTopRight, southBottomIndex, southBottomIndex + 1);

            // Setup north edge geometry
            int northTopLeft = 4 + (x + 2) * (subdivSizeY + 1) - 1;
            int northTopRight = 4 + (x + 1) * (subdivSizeY + 1) - 1;
            int northIndex = verts.Count;
            verts.Add(new Vector3((x + 1) * subdivScaleX, 0, AreaSize.y));
            verts.Add(new Vector3((x) * subdivScaleX, 0, AreaSize.y));
            addQuad(tris, northTopLeft, northTopRight, northIndex, northIndex + 1);
        }
        for (int y = 0; y < subdivSizeY; y++)
        {
            // Setup west edge geometry
            int westTopLeft = 4 + y + 1;
            int westTopRight = 4 + y;
            int westIndex = verts.Count;
            verts.Add(new Vector3(0, 0, (y + 1) * subdivScaleY));
            verts.Add(new Vector3(0, 0, (y) * subdivScaleY));
            addQuad(tris, westTopLeft, westTopRight, westIndex, westIndex + 1);

            // Setup east edge geometry
            int eastTopLeft = 4 + (subdivSizeX) * (subdivSizeY + 1) + y;
            int eastTopRight = 4 + (subdivSizeX) * (subdivSizeY + 1) + y + 1;
            int eastIndex = verts.Count;
            verts.Add(new Vector3(AreaSize.x, 0, (y) * subdivScaleY));
            verts.Add(new Vector3(AreaSize.x, 0, (y + 1) * subdivScaleY));
            addQuad(tris, eastTopLeft, eastTopRight, eastIndex, eastIndex + 1);
        }

        // Assign all data
        Mesh mesh = new Mesh();
        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        groundMF.mesh = mesh;
        groundMR.material = BiomeData.WaterMaterial;

        // Scale down slightly to avoid z-fighting
        waterGO.transform.localScale = new Vector3(0.999f, 0.999f, 0.999f);
        waterGO.transform.localPosition = new Vector3(0.0005f, 0.0005f, 0.0005f);
    }
}
