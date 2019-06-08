using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    [SerializeField] private int xSize;
    [SerializeField] private int ySize;

    private Mesh _mesh;
    private Vector3[] _vertices;
    private Vector2[] _uv;
    private Vector4[] _tangents;

    private void Awake()
    {
        Generate();
    }

    private void Generate()
    {
        var wait = new WaitForSeconds(0.09f);

        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = "Procedural Grid";

        _vertices = new Vector3[(xSize + 1) * (ySize + 1)];
        _uv = new Vector2[_vertices.Length];
        _tangents = new Vector4[_vertices.Length];
        var tangent = new Vector4(1f, 0f, 0f, -1f);

        for (int i = 0, y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++, i++)
            {
                _vertices[i] = new Vector3(x, y);
                _uv[i] = new Vector2((float)x / xSize, (float)y / ySize);
                _tangents[i] = tangent;
            }
        }

        // clockwise orientation (left-hand rule) is considered to be the forward-face, thus visible.
        // We need 6 vertices per quad (2 vertices are coincident)
        var triangles = new int[xSize * ySize * 6];

        for (int y = 0, tileOffset = 0; y < ySize - 1; y++)
        {
            var yOffsetLineAbove = (y + 1) * xSize;
            var yOffsetCurrent   =  y      * xSize;
            for (int x = 0, vertexOffset = 0; x < xSize - 1; x++, tileOffset += 6, vertexOffset++)
            {
                triangles[tileOffset + 0]                             = vertexOffset + yOffsetCurrent;
                triangles[tileOffset + 1] = triangles[tileOffset + 4] = vertexOffset + yOffsetLineAbove;
                triangles[tileOffset + 2] = triangles[tileOffset + 3] = vertexOffset + yOffsetCurrent    + 1;
                triangles[tileOffset + 5]                             = vertexOffset + yOffsetLineAbove  + 1;
            }
        }

        _mesh.vertices = _vertices;
        _mesh.triangles = triangles;
        _mesh.uv = _uv;
        _mesh.tangents = _tangents;
        _mesh.RecalculateNormals();
        _mesh.RecalculateTangents();

    }

    private void OnDrawGizmos()
    {
        if (_vertices == null) return;

        Gizmos.color = Color.black;
        for (int i = 0; i < _vertices.Length; i++)
        {
            Gizmos.DrawSphere(_vertices[i], 0.1f);
        }
    }
}