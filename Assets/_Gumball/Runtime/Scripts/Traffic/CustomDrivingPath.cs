using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    [RequireComponent(typeof(SplineComputer))]
    public class CustomDrivingPath : MonoBehaviour
    {

        [Tooltip("The distance (in metres) that cars will start moving towards the first point of this driving path.")]
        [SerializeField, PositiveValueOnly] private float racerInterpolationDistance = 100;
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private SampleCollection sampleCollection = new();

        public SplineComputer SplineComputer => GetComponent<SplineComputer>();
        public SampleCollection SampleCollection => sampleCollection;
        public SplineSample[] SplineSamples => sampleCollection.samples;
        public float RacerInterpolationDistance => racerInterpolationDistance;
        
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

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            //draw the interpolation distance
            if (racerInterpolationDistance > 0)
            {
                if (SplineSamples == null || SplineSamples.Length == 0)
                    UpdateSplineSampleData();

                SplineSample firstSample = SplineSamples[0];
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(firstSample.position - firstSample.forward * racerInterpolationDistance, 1f);
            }
        }
#endif
    }
}
