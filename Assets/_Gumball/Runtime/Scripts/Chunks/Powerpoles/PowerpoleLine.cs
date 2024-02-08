using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(SplineComputer), typeof(SplineRenderer))]
    public class PowerpoleLine : MonoBehaviour
    {

        private SplineComputer splineComputer => GetComponent<SplineComputer>();
        private SplineRenderer splineRenderer => GetComponent<SplineRenderer>();
        private MeshRenderer meshRenderer => GetComponent<MeshRenderer>();

        [SerializeField, ReadOnly] private PowerpoleLinePosition positionA;
        [SerializeField, ReadOnly] private PowerpoleLinePosition positionB;

        public PowerpoleLinePosition PositionA => positionA;
        public PowerpoleLinePosition PositionB => positionB;

        [SerializeField] private MinMaxFloat minMaxDipAmount = new(0.02f, 1f);
        [SerializeField] private MinMaxFloat minMaxDistanceForDip = new(20, 60);

        public void Initialise(PowerpoleLinePosition positionA, PowerpoleLinePosition positionB, Material material, float width)
        {
            this.positionA = positionA;
            this.positionB = positionB;

            Vector3 firstPosition = positionA.transform.position;
            Vector3 middlePosition = ((positionA.transform.position + positionB.transform.position) / 2f).OffsetY(GetDipAmount());
            Vector3 lastPosition = positionB.transform.position;
            Vector3[] positions = { firstPosition, middlePosition, lastPosition };
            
            splineRenderer._vertexDirection = middlePosition + positionA.transform.up;

            meshRenderer.material = material;
            
            for (int index = 0; index < positions.Length; index++)
            {
                SplinePoint point = new SplinePoint(positions[index], Vector3.zero, Vector3.zero, width, Color.black);
                splineComputer.SetPoint(index, point);
            }
            
            splineComputer.RebuildImmediate();
        }
        
        private float GetDipAmount()
        {
            float distanceBetweenPositionsSqr = Vector3.SqrMagnitude(positionB.transform.position - positionA.transform.position);
            float minDistanceSqr = minMaxDistanceForDip.Min * minMaxDistanceForDip.Min;
            float maxDistanceSqr = minMaxDistanceForDip.Max * minMaxDistanceForDip.Max;
            float difference = maxDistanceSqr - minDistanceSqr;
            float percent = Mathf.Clamp01((distanceBetweenPositionsSqr - minDistanceSqr) / difference);
            
            return -Mathf.Lerp(minMaxDipAmount.Min, minMaxDipAmount.Max, percent);
        }
        
    }
}
