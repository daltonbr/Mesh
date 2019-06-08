using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Cube : MonoBehaviour
{
    [SerializeField] private Vector3Int size;

    private Mesh _mesh;
    private Vector3[] _vertices;

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

        Debug.Log($"[ProceduralCube] vertices: {_vertices.Length.ToString()}");
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

        int v = 0;

        // Loop "rings" from bottom -> up
        for (int y = 0; y <= size.y; y++)
        {
            for (int x = 0; x <= size.x; x++)
            {
                _vertices[v++] = new Vector3(x, y, 0);
            }

            for (int z = 1; z <= size.z; z++)
            {
                _vertices[v++] = new Vector3(size.x, y, z);
            }

            for (int x = size.x - 1; x >= 0; x--)
            {
                _vertices[v++] = new Vector3(x, y, size.z);
            }

            for (int z = size.z - 1; z > 0; z--)
            {
                _vertices[v++] = new Vector3(0, y, z);
            }
        }

        // Top face
        for (int z = 1; z < size.z; z++)
        {
            for (int x = 1; x < size.x; x++)
            {
                _vertices[v++] = new Vector3(x, size.y, z);
            }
        }

        // Bottom face
        for (int z = 1; z < size.z; z++)
        {
            for (int x = 1; x < size.x; x++)
            {
                _vertices[v++] = new Vector3(x, 0, z);
            }
        }

        _mesh.vertices = _vertices;
    }

    private void CreateTriangles()
    {
        int quads = (size.x * size.y + size.x * size.z + size.y * size.z) * 2;
        int[] triangles = new int[quads * 6];

        int ringSize = (size.x + size.z) * 2;
        int t = 0, v = 0;

        // Loop bottom-up, one ring at a time
        for (int y = 0; y < size.y; y++, v++)
        {
            // Ring
            for (int q = 0; q < ringSize - 1; q++, v++)
            {
                t = SetQuad(triangles, t,
                    v, v + 1, v + ringSize, v + ringSize + 1);
            }
            // Close the ring loop (connect to the first triangle of the ring)
            t = SetQuad(triangles, t,
                v, v - ringSize + 1, v + ringSize, v + 1);
        }

        t = CreateTopFace(triangles, t, ringSize);
        t = CreateBottomFace(triangles, t, ringSize);
        _mesh.triangles = triangles;
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

    private void OnDrawGizmos()
    {
        if (_vertices == null) return;

        Gizmos.color = Color.green;
        foreach (var t in _vertices)
        {
            Gizmos.DrawSphere(t, 0.1f);
        }
    }
}