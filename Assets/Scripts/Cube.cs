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
        StartCoroutine(Generate());
    }

    private IEnumerator Generate()
    {
        var wait = new WaitForSeconds(0.09f);

        GetComponent<MeshFilter>().mesh = _mesh = new Mesh();
        _mesh.name = "Procedural Cube";

        // We don't want repeating vertices
        int cornerVertices = 8;
        int edgeVertices = 4 * (size.x + size.y + size.z);
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
                yield return wait;
            }

            for (int z = 1; z <= size.z; z++)
            {
                _vertices[v++] = new Vector3(size.x, y, z);
                yield return wait;
            }

            for (int x = size.x; x >= 0; x--)
            {
                _vertices[v++] = new Vector3(x, y, size.z);
                yield return wait;
            }

            for (int z = size.z - 1; z > 0; z--)
            {
                _vertices[v++] = new Vector3(0, y, z);
                yield return wait;
            }
        }

        // Bottom face
        for (int z = 1; z < size.z; z++)
        {
            for (int x = 1; x < size.x; x++)
            {
                _vertices[v++] = new Vector3(x, 0, z);
                yield return wait;
            }
        }

        // Top face
        for (int z = 1; z < size.z; z++)
        {
            for (int x = 1; x < size.x; x++)
            {
                _vertices[v++] = new Vector3(x, size.y, z);
                yield return wait;
            }
        }

        _mesh.vertices = _vertices;

        Debug.Log($"[ProceduralCube] vertices: {_vertices.Length.ToString()}");
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