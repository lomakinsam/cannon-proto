using UnityEngine;

public static class ProjectileMeshGenerator
{
    private const float meshScale = 1f;
    private const float extents = 0.5f;

    private const float noiseScale = 1f;
    private const float noiseFrequency = 100f;
    private const float maxOffset = 0.25f;

    private static Mesh GenerateCube(bool recalculate = true)
    {
        Mesh cubeMesh = new ();

        cubeMesh.vertices = GenerateDefaultVerticesPosition();
        cubeMesh.triangles = GenerateTriangles();

        if (recalculate)
        {
            cubeMesh.RecalculateNormals();
            cubeMesh.RecalculateBounds();
        }

        return cubeMesh;
    }

    private static Vector3[] GenerateDefaultVerticesPosition()
    {
        Vector3[] vertices = new Vector3[24];

        // right
        vertices[0] = new Vector3( extents, -extents, -extents) * meshScale;
        vertices[1] = new Vector3( extents,  extents, -extents) * meshScale;
        vertices[2] = new Vector3( extents,  extents,  extents) * meshScale;
        vertices[3] = new Vector3( extents, -extents,  extents) * meshScale;

        // left
        vertices[4] = new Vector3(-extents, -extents, -extents) * meshScale;
        vertices[5] = new Vector3(-extents,  extents, -extents) * meshScale;
        vertices[6] = new Vector3(-extents,  extents,  extents) * meshScale;
        vertices[7] = new Vector3(-extents, -extents,  extents) * meshScale;

        // forward
        vertices[8]  = new Vector3(-extents, -extents,  extents) * meshScale;
        vertices[9]  = new Vector3(-extents,  extents,  extents) * meshScale;
        vertices[10] = new Vector3( extents,  extents,  extents) * meshScale;
        vertices[11] = new Vector3( extents, -extents,  extents) * meshScale;

        // back
        vertices[12] = new Vector3(-extents, -extents, -extents) * meshScale;
        vertices[13] = new Vector3(-extents,  extents, -extents) * meshScale;
        vertices[14] = new Vector3( extents,  extents, -extents) * meshScale;
        vertices[15] = new Vector3( extents, -extents, -extents) * meshScale;

        // top
        vertices[16] = new Vector3(-extents,  extents, -extents) * meshScale;
        vertices[17] = new Vector3(-extents,  extents,  extents) * meshScale;
        vertices[18] = new Vector3( extents,  extents,  extents) * meshScale;
        vertices[19] = new Vector3( extents,  extents, -extents) * meshScale;

        // bottom
        vertices[20] = new Vector3(-extents, -extents, -extents) * meshScale;
        vertices[21] = new Vector3(-extents, -extents,  extents) * meshScale;
        vertices[22] = new Vector3( extents, -extents,  extents) * meshScale;
        vertices[23] = new Vector3( extents, -extents, -extents) * meshScale;

        return vertices;
    }

    private static int[] GenerateTriangles()
    {
        int[] triangles = new int[36];

        // right
        triangles[0] = 0; triangles[1] = 1; triangles[2] = 3;
        triangles[3] = 3; triangles[4] = 1; triangles[5] = 2;

        // left
        triangles[6] = 4; triangles[7] = 7; triangles[8] = 5;
        triangles[9] = 5; triangles[10] = 7; triangles[11] = 6;

        // forward
        triangles[12] = 8; triangles[13] = 11; triangles[14] = 9;
        triangles[15] = 9; triangles[16] = 11; triangles[17] = 10;

        // back
        triangles[18] = 12; triangles[19] = 13; triangles[20] = 15;
        triangles[21] = 15; triangles[22] = 13; triangles[23] = 14;

        // top
        triangles[24] = 16; triangles[25] = 17; triangles[26] = 19;
        triangles[27] = 19; triangles[28] = 17; triangles[29] = 18;

        // bottom
        triangles[30] = 20; triangles[31] = 23; triangles[32] = 21;
        triangles[33] = 21; triangles[34] = 23; triangles[35] = 22;

        return triangles;
    }

    public static Mesh GenerateMesh()
    {
        Mesh mesh = GenerateCube(false);
        RegenerateVertexNoise(ref mesh);

        return mesh;
    }

    public static void RegenerateVertexNoise(ref Mesh mesh)
    {
        if (mesh.vertexCount != 24)
        {
            Debug.LogWarning("Invalid mesh! Returned unchanged mesh");
            return;
        }

        Vector3 seed = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        Vector3[] vertices = GenerateDefaultVerticesPosition();

        for (int i = 0; i < vertices.Length; i++)
        {
            float Xoffset = noiseScale * maxOffset * ((Mathf.PerlinNoise((vertices[i].x - vertices[i].y + seed.x) * noiseFrequency, 0) - 0.5f) * 2);
            float Yoffset = noiseScale * maxOffset * ((Mathf.PerlinNoise(0, (vertices[i].y - vertices[i].z + seed.y) * noiseFrequency) - 0.5f) * 2);
            float Zoffset = noiseScale * maxOffset * ((Mathf.PerlinNoise((vertices[i].z + vertices[i].x + seed.z) * noiseFrequency, (vertices[i].z - vertices[i].y + seed.z ) * noiseFrequency) - 0.5f) * 2);

            vertices[i] += new Vector3(Xoffset, Yoffset, Zoffset);
        }

        mesh.vertices = vertices;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}