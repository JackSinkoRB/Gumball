using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VertexInstanceStreamData
{

    public Color[] Colors;
    public List<Vector4> Uv0;
    public List<Vector4> Uv1;
    public List<Vector4> Uv2;
    public List<Vector4> Uv3;
    public Vector3[] Positions;
    public Vector3[] Normals;
    public Vector4[] Tangents;

}
