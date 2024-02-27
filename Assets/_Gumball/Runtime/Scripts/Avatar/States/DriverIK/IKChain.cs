using System;
using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEditor;
using UnityEngine;

namespace Gumball
{
    [Serializable]
    public class IkChain
    {
        
        public int chainLength;
        public Transform bone;
        public Transform target;
        public Transform pole;

        public int iterations = 10;
        public float delta = 0.001f;

        public bool showDebug;

        private float[] bonesLength;
        private float completeLength;
        private Transform[] bones;
        private Vector3[] positions;
        private Vector3[] startDirectionSucc;
        private Quaternion[] startRotationBone;

        public void Initialise(Transform target = null)
        {
            if (bone == null)
            {
                Debug.LogError("No bone for ik chain");
                return;
            }

            bones = new Transform[chainLength + 1];
            positions = new Vector3[chainLength + 1];
            bonesLength = new float[chainLength];
            startDirectionSucc = new Vector3[chainLength + 1];
            startRotationBone = new Quaternion[chainLength + 1];

            //init fields
            if (target != null)
            {
                this.target = target;
            }
            
            if (target == null)
            {
                target = new GameObject(bone.gameObject.name + " target").transform;
                target.position = bone.transform.position;
            }

            completeLength = 0;

            //init data
            Transform current = bone.transform;
            for (int i = bones.Length - 1; i >= 0; i--)
            {
                bones[i] = current;
                startRotationBone[i] = current.rotation;

                if (i == bones.Length - 1)
                {
                    startDirectionSucc[i] = target.position - current.position;
                }
                else
                {
                    startDirectionSucc[i] = bones[i + 1].position - current.position;
                    bonesLength[i] = (bones[i + 1].position - current.position).magnitude;
                    completeLength += bonesLength[i];
                }

                current = current.parent;
            }
        }

        internal void ResolveIK()
        {
            if (target == null)
                return;

            if (bonesLength == null || bonesLength.Length != chainLength)
                Initialise();

            //Get positions
            for (int i = 0; i < bones.Length; ++i)
                positions[i] = bones[i].position;

            //Calculation

            if ((target.position - bones[0].position).sqrMagnitude >= completeLength * completeLength)
            {
                //Extended
                Vector3 direction = (target.position - positions[0]).normalized;

                for (int i = 1; i < positions.Length; ++i)
                    positions[i] = positions[i - 1] + direction * bonesLength[i - 1];
            }
            else
            {
                for (int i = 0; i < iterations; ++i)
                {
                    //back
                    for (int p = positions.Length - 1; p > 0; p--)
                    {
                        if (p == positions.Length - 1)
                            positions[p] = target.position; //set to target
                        else
                            positions[p] = positions[p + 1] + (positions[p] - positions[p + 1]).normalized * bonesLength[p];
                    }

                    //forward
                    for (int p = 1; p < positions.Length; ++p)
                        positions[p] = positions[p - 1] + (positions[p] - positions[p - 1]).normalized * bonesLength[p - 1];

                    if ((positions[^1] - target.position).sqrMagnitude < delta * delta)
                        break;
                }
            }

            //move towards pole
            if (pole != null)
            {
                for (int i = 1; i < positions.Length - 1; ++i)
                {
                    Plane plane = new Plane(positions[i + 1] - positions[i - 1], positions[i - 1]);
                    Vector3 projectedPole = plane.ClosestPointOnPlane(pole.position);
                    Vector3 projectedBone = plane.ClosestPointOnPlane(positions[i]);
                    float angle = Vector3.SignedAngle(projectedBone - positions[i - 1], projectedPole - positions[i - 1], plane.normal);

                    positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (positions[i] - positions[i - 1]) + positions[i - 1];
                }
            }

            //Set positions and rotation
            for (int i = 0; i < positions.Length; ++i)
            {
                if (i == positions.Length - 1)
                {
                    IKPositionData ikPositionData = target.GetComponent<IKPositionData>();
                    if (ikPositionData != null && ikPositionData.EndBoneCopiesRotation)
                        bones[i].rotation = target.rotation;
                }
                else
                    bones[i].rotation = Quaternion.FromToRotation(startDirectionSucc[i], positions[i + 1] - positions[i]) * startRotationBone[i];

                bones[i].position = positions[i];
            }
        }

        internal void DrawGizmos()
        {
            if (!showDebug || bone == null)
                return;

#if UNITY_EDITOR
            Transform current = bone.transform;
            if (current == null)
                return;

            Gizmos.color = Color.green;
            for (int i = 0; i < chainLength && current != null && current.parent != null; i++)
            {
                Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(0.01f, Vector3.Distance(current.parent.position, current.position), 0.01f));
                Handles.color = Color.green;
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);

                current = current.parent;
            }
#endif
        }
    }
}