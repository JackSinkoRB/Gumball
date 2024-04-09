using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class VertexInstanceStreamData
{

    [HideInInspector] public Color[] Colors;
    [HideInInspector] public List<Vector4> Uv0;
    [HideInInspector] public List<Vector4> Uv1;
    [HideInInspector] public List<Vector4> Uv2;
    [HideInInspector] public List<Vector4> Uv3;
    [HideInInspector] public Vector3[] Positions;
    [HideInInspector] public Vector3[] Normals;
    [HideInInspector] public Vector4[] Tangents;

}
