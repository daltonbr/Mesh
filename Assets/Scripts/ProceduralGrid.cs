using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralGrid : MonoBehaviour
{
    [SerializeField] private int xSize;
    [SerializeField] private int ySize;

    private Vector3[] _vertices;
    private Mesh _mesh;

    private void Awake()
    {
        StartCoroutine(Generate());
        //Generate();
    }

    private IEnumerator Generate()
    {
        var wait = new WaitForSeconds(0.09f);

        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = "Procedural Grid";

        _vertices = new Vector3[(xSize + 1) * (ySize + 1)];

        for (int i = 0, y = 0; y < ySize; y++)
        {
            for (int x = 0; x < xSize; x++, i++)
            {
                _vertices[i] = new Vector3(x, y);

            }
        }

        _mesh.vertices = _vertices;

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
                _mesh.triangles = triangles;
                yield return wait;
            }
        }


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