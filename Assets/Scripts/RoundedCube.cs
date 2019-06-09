using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoundedCube : MonoBehaviour
{
    [SerializeField] private Vector3Int size;
    [SerializeField] private float roundness;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector3[] _normals;
    private Color32[] _cubeUV;

    private void Awake()
    {
        Generate();
    }

    WaitForSeconds wait = new WaitForSeconds(0.1f);
    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = "Procedural Cube";

        CreateVertices();
        CreateTriangles();
        CreateColliders();

        Debug.Log($"[ProceduralCube] vertices: {_vertices.Length.ToString()}");
    }

    private void CreateColliders()
    {
        AddBoxCollider(size.x, size.y - roundness * 2, size.z - roundness * 2);
        AddBoxCollider(size.x - roundness * 2, size.y, size.z - roundness * 2);
        AddBoxCollider(size.x - roundness * 2, size.y - roundness * 2, size.z);

        Vector3 min = Vector3.one * roundness;
        Vector3 half = new Vector3(size.x, size.y, size.z) * 0.5f;
        Vector3 max = new Vector3(size.x, size.y, size.z) - min;

        AddCapsuleCollider(0, half.x, min.y, min.z);
        AddCapsuleCollider(0, half.x, min.y, max.z);
        AddCapsuleCollider(0, half.x, max.y, min.z);
        AddCapsuleCollider(0, half.x, max.y, max.z);

        AddCapsuleCollider(1, min.x, half.y, min.z);
        AddCapsuleCollider(1, min.x, half.y, max.z);
        AddCapsuleCollider(1, max.x, half.y, min.z);
        AddCapsuleCollider(1, max.x, half.y, max.z);

        AddCapsuleCollider(2, min.x, min.y, half.z);
        AddCapsuleCollider(2, min.x, max.y, half.z);
        AddCapsuleCollider(2, max.x, min.y, half.z);
        AddCapsuleCollider(2, max.x, max.y, half.z);
    }

    private void AddBoxCollider(float x, float y, float z)
    {
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();
        collider.size = new Vector3(x, y, z);
    }

    private void AddCapsuleCollider(int direction, float x, float y, float z)
    {
        CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        collider.center = new Vector3(x, y, z);
        collider.direction = direction;
        collider.radius = roundness;
        collider.height = collider.center[direction] * 2f;
    }

    private void CreateVertices()
    {
        // We don't want repeating vertices
        int cornerVertices = 8;
        int edgeVertices = 4 * (size.x + size.y + size.z - 3);
        int faceVertices = (  (size.x - 1) * (size.y - 1)
                            + (size.x - 1) * (size.z - 1)
                            + (size.y - 1) * (size.z - 1)) * 2;

        _vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        _normals = new Vector3[_vertices.Length];
        _cubeUV = new Color32[_vertices.Length];

        int v = 0;

        // Loop "rings" from bottom -> up
        for (int y = 0; y <= size.y; y++)
        {
            for (int x = 0; x <= size.x; x++)
            {
                SetVertex(v++, x, y, 0);
            }

            for (int z = 1; z <= size.z; z++)
            {
                SetVertex(v++, size.x, y, z);
            }

            for (int x = size.x - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, size.z);
            }

            for (int z = size.z - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
            }
        }

        // Top face
        for (int z = 1; z < size.z; z++)
        {
            for (int x = 1; x < size.x; x++)
            {
                SetVertex(v++, x, size.y, z);
            }
        }

        // Bottom face
        for (int z = 1; z < size.z; z++)
        {
            for (int x = 1; x < size.x; x++)
            {
                SetVertex(v++, x, 0, z);
            }
        }

        _mesh.vertices = _vertices;
        _mesh.normals = _normals;
        _mesh.colors32 = _cubeUV;

    }

    private void SetVertex(int index, int x, int y, int z)
    {
        Vector3 inner = _vertices[index] = new Vector3(x, y, z);

        if (x < roundness)
        {
            inner.x = roundness;
        }
        else if (x > size.x - roundness)
        {
            inner.x = size.x - roundness;
        }

        if (y < roundness)
        {
            inner.y = roundness;
        }
        else if (y > size.y - roundness)
        {
            inner.y = size.y - roundness;
        }

        if (z < roundness)
        {
            inner.z = roundness;
        }
        else if (z > size.z - roundness)
        {
            inner.z = size.z - roundness;
        }

        _normals[index] = (_vertices[index] - inner).normalized;
        _vertices[index] = inner + _normals[index] * roundness;
        _cubeUV[index] = new Color32((byte)x, (byte)y, (byte)z, 0);
    }

    private void CreateTriangles()
    {
        int[] trianglesZ = new int[(size.x * size.y) * 12];
        int[] trianglesX = new int[(size.y * size.z) * 12];
        int[] trianglesY = new int[(size.x * size.z) * 12];
        int quads = (size.x * size.y + size.x * size.z + size.y * size.z) * 2;
        //int[] triangles = new int[quads * 6];

        int ringSize = (size.x + size.z) * 2;
        int tZ = 0, tX = 0, tY = 0, v = 0;
        //int t = 0, v = 0;

        // Loop bottom-up, one ring at a time
        for (int y = 0; y < size.y; y++, v++)
        {
            // Ring
            for (int q = 0; q < size.x; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ,
                    v, v + 1, v + ringSize, v + ringSize + 1);
            }
            for (int q = 0; q < size.z; q++, v++)
            {
                tX = SetQuad(trianglesX, tX,
                    v, v + 1, v + ringSize, v + ringSize + 1);
            }
            for (int q = 0; q < size.x; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ,
                    v, v + 1, v + ringSize, v + ringSize + 1);
            }
            for (int q = 0; q < size.z - 1; q++, v++)
            {
                tX = SetQuad(trianglesX, tX,
                    v, v + 1, v + ringSize, v + ringSize + 1);
            }
            // Close the ring loop (connect to the first triangle of the ring)
            tX = SetQuad(trianglesX, tX,
                v, v - ringSize + 1, v + ringSize, v + 1);
        }

        tY = CreateTopFace(trianglesY, tY, ringSize);
        tY = CreateBottomFace(trianglesY, tY, ringSize);

        _mesh.subMeshCount = 3;
        _mesh.SetTriangles(trianglesZ, 0);
        _mesh.SetTriangles(trianglesX, 1);
        _mesh.SetTriangles(trianglesY, 2);
    }

    private int CreateTopFace(int[] triangles, int t, int ringSize)
    {
        int v = ringSize * size.y;
        for (int x = 0; x < size.x - 1; x++, v++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ringSize - 1, v + ringSize);
        }
        t = SetQuad(triangles, t, v, v + 1, v + ringSize - 1, v + 2);

        int vMin = ringSize * (size.y + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        for (int z = 1; z < size.z - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + size.x - 1);

            for (int x = 1; x < size.x - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + size.x - 1, vMid + size.x);
            }
            t = SetQuad(triangles, t, vMid, vMax, vMid + size.x - 1, vMax + 1);
        }

        // last row
        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for (int x = 1; x < size.x - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        }
        t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
        return t;
    }

    private int CreateBottomFace (int[] triangles, int t, int ringSize)
    {
        int v = 1;
        int vMid = _vertices.Length - (size.x - 1) * (size.z - 1);
        t = SetQuad(triangles, t, ringSize - 1, vMid, 0, 1);
        for (int x = 1; x < size.x - 1; x++, v++, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

        int vMin = ringSize - 2;
        vMid -= size.x - 2;
        int vMax = v + 2;

        for (int z = 1; z < size.z - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + size.x - 1, vMin + 1, vMid);
            for (int x = 1; x < size.x - 1; x++, vMid++)
            {
                t = SetQuad(triangles, t,
                    vMid + size.x - 1, vMid + size.x, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + size.x - 1, vMax + 1, vMid, vMax);
        }

        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < size.x - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

        return t;
    }

    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        /*
         *  Triangles {0, 1, 2} & {3, 4, 5}
         *  They need to be clockwise
         *     v01            v11
         *      *--------------*
         *      |1&4          5|
         *      |              |
         *      |              |
         *      |              |
         *      |0          2&3|
         *      *--------------*
         *     v00            v10
         */
        triangles[i    ]                    = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5]                    = v11;
        return i + 6;
    }

//    private void OnDrawGizmos()
//    {
//        if (_vertices == null) return;
//
//        for (var index = 0; index < _vertices.Length; index++)
//        {
//            Gizmos.color = Color.black;
//            Gizmos.DrawSphere(_vertices[index], 0.1f);
//            Gizmos.color = Color.yellow;
//            Gizmos.DrawRay(_vertices[index], _normals[index]);
//        }
//    }
}