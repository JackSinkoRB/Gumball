using UnityEngine;

[RequireComponent(typeof(BezierPath))]
public class BezierPipeMesh : MonoBehaviour
{
    public int points = 3;
    public float radius = 0.1f;
    public float rotation = 0;

    [ContextMenu("Generate")]
    public void Generate()
    {
        BezierPath path = GetComponent<BezierPath>();
        path.Rebuild();
        Vector3[] bezpoints = new Vector3[path.cachedPoints.Count];
        for (int i = 0; i < bezpoints.Length; ++i)
            bezpoints[i] = path.transform.InverseTransformPoint(path.cachedPoints[i].position);

        int vertexcount = bezpoints.Length * (points + 1);
        Vector3[] positions = new Vector3[vertexcount];
        Vector3[] normals = new Vector3[vertexcount];
        Vector2[] uvs = new Vector2[vertexcount];
        int[] indices = new int[(bezpoints.Length - 1) * points * 6];

        Vector2[] angles = new Vector2[points + 1];
        for (int i = 0; i < points + 1; ++i)
        {
            float angle = (rotation + 360f * i / points) * Mathf.Deg2Rad;
            angles[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        // vertices
        for (int i = 0; i < bezpoints.Length; ++i)
        {
            Vector3 center = bezpoints[i];
            Vector3 forward;
            if (i == 0 || bezpoints.Length <= 2)
                forward = bezpoints[i + 1] - center;
            else if (i < bezpoints.Length - 1)
                forward = bezpoints[i + 1] - bezpoints[i - 1];
            else
                forward = center - bezpoints[i - 1];
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized * radius;
            Vector3 up = Vector3.Cross(forward, right).normalized * radius;

            for (int p = 0; p < points + 1; ++p)
            {
                Vector3 offset = right * angles[p].x + up * angles[p].y;

                int idx = i * (points + 1) + p;
                positions[idx] = center + offset;
                normals[idx] = offset.normalized;
                uvs[idx] = new Vector2(i, (float)p / (points + 1));
            }
        }

        // indices
        for (int i = 0; i < bezpoints.Length - 1; ++i)
        {
            for (int p = 0; p < points; ++p)
            {
                int istart = (i * points + p) * 6;
                int vstart = i * (points + 1) + p;

                int v0 = vstart;
                int v1 = vstart + points + 1;
                int v2 = vstart + points + 2;
                int v3 = vstart + 1;

                indices[istart + 0] = v0;
                indices[istart + 1] = v2;
                indices[istart + 2] = v1;
                indices[istart + 3] = v0;
                indices[istart + 4] = v3;
                indices[istart + 5] = v2;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "BezierPipeMesh";
        mesh.vertices = positions;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = indices;
        mesh.Optimize();
        mesh.UploadMeshData(true);

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
