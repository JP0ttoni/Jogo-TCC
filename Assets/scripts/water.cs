using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class water : MonoBehaviour
{
    public float waveHeight = 0.3f;
    public float waveSpeed = 1f;
    public float waveFrequency = 1f;

    Mesh mesh;
    Vector3[] vertices;
    Vector3[] baseVertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        baseVertices = mesh.vertices;
        vertices = mesh.vertices;
    }

    void Update()
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = baseVertices[i];
            v.y = Mathf.Sin(Time.time * waveSpeed + v.x * waveFrequency) * waveHeight;
            vertices[i] = v;
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }
}
