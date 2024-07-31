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
        
        [Header("Debugging")]
        [SerializeField, ReadOnly] private SampleCollection sampleCollection = new();

        public SplineComputer SplineComputer => GetComponent<SplineComputer>();
        public SampleCollection SampleCollection => sampleCollection;
        public SplineSample[] SplineSamples => sampleCollection.samples;
        
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
        
    }
}
