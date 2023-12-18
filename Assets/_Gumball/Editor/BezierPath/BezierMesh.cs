using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(BezierPath))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class BezierMesh : MonoBehaviour
{
    public float textureAspectRatio = 0.25f;
    public float width = 3;
    public float distance = 0;
    public Vector3 groundOffset = new Vector3(0, 0.01f, 0);
    Mesh _mesh = null;
    Material _material;

    BezierPath _path;

    // cached mesh stuff
    List<Vector3> _positions = new List<Vector3>();
    List<Vector2> _uvs = new List<Vector2>();
    List<Vector3> _normals = new List<Vector3>();
    List<int> _indices = new List<int>();

    void Update()
    {
#if UNITY_EDITOR
        Generate();
#endif

        if (_material == null)
            _material = GetComponent<MeshRenderer>().sharedMaterial;
        if (_path == null)
            _path = GetComponent<BezierPath>();

        float texsize = width / textureAspectRatio;
        distance = Mathf.Clamp(distance, 0, _path.totalDistance - texsize);

        float invtexsize = textureAspectRatio / width;
        Vector4 texst = new Vector4(
            1,
            invtexsize,
            0,
            -(invtexsize * distance - invtexsize)
            );
        _material.SetVector("_MainTex_ST", texst);
    }

    [ContextMenu("Generate")]
    void Generate()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
            _mesh.name = "BezierMesh";
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }

        if (_path == null)
            _path = GetComponent<BezierPath>();
        if (_path.cachedPoints.Count < 2)
            return;

        _positions.Clear();
        _uvs.Clear();
        _normals.Clear();
        for (int i = 0; i < _path.cachedPoints.Count; ++i)
        {
            BezierPathPoint point = _path.cachedPoints[i];

            Vector3 p1 = i == 0 ? point.position : _path.cachedPoints[i - 1].position;
            Vector3 p2 = i == 0 ? _path.cachedPoints[i + 1].position : point.position;
            Vector3 offset = Vector3.Cross(Vector3.up, p2 - p1).normalized * (width * 0.5f);
            offset.y = 0;
            _positions.Add(point.position + groundOffset - offset);
            _positions.Add(point.position + groundOffset + offset);

            _uvs.Add(new Vector2(0, point.distance));
            _uvs.Add(new Vector2(1, point.distance));

            _normals.Add(Vector3.up);
            _normals.Add(Vector3.up);
        }

        _indices.Clear();
        for (int i = 0; i < _path.cachedPoints.Count - 1; ++i)
        {
            _indices.Add(i * 2 + 0);
            _indices.Add(i * 2 + 2);
            _indices.Add(i * 2 + 3);

            _indices.Add(i * 2 + 0);
            _indices.Add(i * 2 + 3);
            _indices.Add(i * 2 + 1);
        }

        _mesh.SetVertices(_positions);
        _mesh.SetNormals(_normals);
        _mesh.SetUVs(0, _uvs);
        _mesh.SetTriangles(_indices, 0);
    }
}