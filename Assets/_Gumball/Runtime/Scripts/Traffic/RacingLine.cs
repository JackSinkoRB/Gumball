using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(SplineComputer))]
    public class RacingLine : MonoBehaviour
    {
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private SampleCollection sampleCollection = new();

        public SplineComputer SplineComputer => GetComponent<SplineComputer>();
        public SampleCollection SampleCollection => sampleCollection;

        private void OnEnable()
        {
            UpdateSplineSampleData();
            
            SplineComputer.onRebuild += UpdateSplineSampleData;
        }

        private void OnDisable()
        {
            SplineComputer.onRebuild -= UpdateSplineSampleData;
        }

        public void UpdateSplineSampleData()
        {
            SplineComputer.GetSamples(sampleCollection);
        }
        
        public float GetOffsetFromRacingLine(Vector3 fromPoint)
        {
            var (splineSample, distanceSqr) = SampleCollection.GetClosestSampleOnSpline(fromPoint);
            float distance = Mathf.Sqrt(distanceSqr);
            
            //is the position to the left or right of the spline?
            bool isRight = fromPoint.IsFurtherInDirection(splineSample.position, splineSample.right);
            float offsetDirection = isRight ? 1 : -1;
            
            return distance * offsetDirection;
        }
        
    }
}
