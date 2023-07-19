using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class TireModification : MonoBehaviour
    {
        [SerializeField] private MeshFilter filter;
        [SerializeField] private MeshRenderer render;

        [Range(-1, 2f)] public float width = 1f;
        [Range(-1f, 2f)] public float height = 1f;
        [Range(0f, 1f)] private float minCutoff = 0.5f;
        [Range(0f, 1f)] public float grip = 0f;

        [SerializeField] private Mesh mesh;
        private Vector3[] vertices;
        private Vector3[] origin;

        private float setGrip;
        private float setWidth;
        private float setHeight;
        const float centerDistance = 0.002856576f;
        const float maxDistance = 0.003415307f;

        public Color[] meshColors;

        private int[] colourMask;
        private Vector3 fixedScale = new Vector3(1.035f, 1.035f, 1.31f);


        private void Awake()
        {
            mesh = Instantiate(filter.sharedMesh);

            filter.mesh = mesh;

            vertices = mesh.vertices;
            origin = mesh.vertices; //Store this as a master reference somewhere, instead of needing four copies

            setWidth = width;
            setHeight = height;

            SetColourMask(mesh);
            FindCenter(mesh.vertices);
            setGrip = 1f;
            transform.localScale = fixedScale;
        }


        private void SetColourMask(Mesh mesh)
        {
            if (mesh.colors.Length < mesh.vertices.Length)
            {
                mesh.colors = meshColors;
            }

            var white = Color.white;
            var red = Color.red;
            var blue = Color.blue;

            var whiteCount = 0;
            var redCount = 0;
            var blueCount = 0;

            var results = new List<int>();

            var centerDistance = 0f;
            var sampleCount = 0;
            if (mesh.vertices.Length > 0)
            {
                for (int i = 0; i < mesh.vertices.Length; ++i)
                {
                    if (CompareColour(mesh.colors[i], white))
                    {
                        centerDistance += (mesh.vertices[i] - Vector3.zero).magnitude;
                        sampleCount++;

                        whiteCount++;
                        results.Add(0);
                        continue;
                    }

                    if (CompareColour(mesh.colors[i], red))
                    {
                        redCount++;
                        results.Add(1);
                        continue;
                    }

                    if (CompareColour(mesh.colors[i], blue))
                    {
                        blueCount++;
                        results.Add(2);
                        continue;
                    }
                }
            }


            colourMask = results.ToArray();
        }

        private bool CompareColour(Color color, Color target)
        {
            if (Mathf.Abs(Mathf.RoundToInt(color.r * 1000f) - Mathf.RoundToInt(target.r * 1000f)) > 100f) return false;
            if (Mathf.Abs(Mathf.RoundToInt(color.g * 1000f) - Mathf.RoundToInt(target.g * 1000f)) > 100f) return false;
            if (Mathf.Abs(Mathf.RoundToInt(color.b * 1000f) - Mathf.RoundToInt(target.b * 1000f)) > 100f) return false;

            return true;
        }

        private void FindCenter(Vector3[] vertices)
        {
            var center = Vector3.zero;
            var sampleCount = 0;
            for (int i = 0; i < vertices.Length; ++i)
            {
                if (colourMask[i] != 0) continue;
                var p = transform.TransformPoint(vertices[i]);
                if (p.z < 0.09f) continue;

                center += p;
                sampleCount++;
            }
        }

        void AdjustGrip(float newGrip)
        {
            if (Mathf.Abs(setGrip - newGrip) <= 0f) return;
            setGrip = newGrip;
            render.material.SetFloat("_Grip", newGrip);
        }

        public void ApplyChanges(float w, float h)
        {
            AdjustSize(1 + w, 1 + h);
        }

        void AdjustSize(float newWidth, float newHeight)
        {
            if (Mathf.Abs(setWidth - width) <= 0f && Mathf.Abs(setHeight - height) <= 0f) return;
            //setWidth = Mathf.Clamp(newWidth, 0.85f, 1.3f);
            //setHeight = Mathf.Clamp(newHeight, 1f, 1.3f);
            setWidth = newWidth;
            setHeight = newHeight;
            for (int i = 0; i < vertices.Length; ++i)
            {
                if (colourMask[i] == 0) continue;
                var p = origin[i];
                var home = Vector3.zero;

                var dir = (p - home).normalized;

                var dist = (origin[i] - home).magnitude;

                var ratio = Mathf.InverseLerp(centerDistance, maxDistance, dist);
                if (ratio < minCutoff)
                {
                    vertices[i] = origin[i];
                    continue;
                }

                var calcHeight = dist * setHeight;
                if (calcHeight < centerDistance) calcHeight = centerDistance;

                var endHeight = home + (dir * calcHeight);
                var endWidth = home + (dir * dist * setWidth);

                p = home;
                p.x = endHeight.x;
                p.y = endHeight.y;
                p.z = endWidth.z;

                vertices[i] = p;
            }

            mesh.vertices = vertices;
        }

        void AdjustWidth()
        {
            setWidth = width;

            for (int i = 0; i < vertices.Length; ++i)
            {
                if (colourMask[i] == 0) continue;
                var p = origin[i];
                var home = Vector3.zero;

                var dir = (p - home).normalized;

                var dist = (origin[i] - home).magnitude;

                var ratio = Mathf.InverseLerp(centerDistance, maxDistance, dist);
                if (ratio < minCutoff)
                {
                    vertices[i] = origin[i];
                    continue;
                }

                dist *= setWidth;

                p = home + (dir * dist);
                /*
                if (widthScale.x <= 0) p.x = origin[i].x;
                if (widthScale.y <= 0) p.y = origin[i].y;
                if (widthScale.z <= 0) p.z = origin[i].z;
                */
                p.x = origin[i].x;
                p.y = origin[i].y;

                vertices[i] = p;
                //vertices[i] = Vector3.Lerp(vertices[i], p, ratio);
            }

            mesh.vertices = vertices;
        }


        void AdjustHeight()
        {
            setHeight = height;

            for (int i = 0; i < vertices.Length; ++i)
            {
                if (colourMask[i] == 0) continue;
                var p = origin[i];
                var home = Vector3.zero;

                var dir = (p - home).normalized;
                //dir.y = 0;

                var dist = (origin[i] - home).magnitude * setHeight;
                if (dist < centerDistance) dist = centerDistance;

                p = home + (dir * dist);
                p.z = origin[i].z;

                vertices[i] = p;
            }

            mesh.vertices = vertices;
        }
    }
}