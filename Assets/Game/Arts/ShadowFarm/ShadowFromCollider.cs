using UnityEngine;
using System.Collections.Generic;
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ShadowFromCollider : MonoBehaviour
{
    public Material shadowMaterial; // <- Material warna hitam transparan

    void Start()
    {
        var collider = GetComponent<PolygonCollider2D>();
        var mesh = new Mesh();

        // Convert Vector2[] to Vector3[]
        Vector2[] points = collider.points;
        Vector3[] vertices = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            vertices[i] = points[i];
        }

        // Simple triangle fan (assumes convex polygon!)
        List<int> triangles = new List<int>();
        for (int i = 1; i < points.Length - 1; i++)
        {
            triangles.Add(0);
            triangles.Add(i);
            triangles.Add(i + 1);
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles.ToArray();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().material = shadowMaterial;
    }
}