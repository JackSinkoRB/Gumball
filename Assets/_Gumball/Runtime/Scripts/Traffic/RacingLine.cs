using System;
using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

namespace Gumball
{
    public class RacingLine : MonoBehaviour
    {
        
        [SerializeField] private SplineComputer splineComputer;
        [SerializeField] private SampleCollection sampleCollection = new();

        public SplineComputer SplineComputer => splineComputer;
        public SampleCollection SampleCollection => sampleCollection;

        private void OnEnable()
        {
            UpdateSplineSampleData();
            
            splineComputer.onRebuild += UpdateSplineSampleData;
        }

        private void OnDisable()
        {
            splineComputer.onRebuild -= UpdateSplineSampleData;
        }

        public void UpdateSplineSampleData()
        {
            splineComputer.GetSamples(sampleCollection);
        }
        
    }
}
