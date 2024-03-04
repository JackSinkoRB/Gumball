using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using UnityEngine;

namespace Gumball
{
    public static class SplineComputerUtils
    {

        public static void InsertPoint(this SplineComputer splineComputer, int index, SplinePoint splinePoint)
        {
            //move all up 1
            for (int count = splineComputer.pointCount; count > index; count--)
            {
                splineComputer.SetPoint(count, splineComputer.GetPoint(count - 1));
            }
            
            splineComputer.SetPoint(index, splinePoint);
        }
        
        public static (int, float) GetClosestSampleIndexOnSpline(this SampleCollection sampleCollection, Vector3 fromPoint)
        {
            float closestDistanceSqr = Mathf.Infinity;
            int closestSampleIndex = -1;
            for (int index = 0; index < sampleCollection.samples.Length; index++)
            {
                SplineSample sample = sampleCollection.samples[index];
                
                float distance = Vector3.SqrMagnitude(fromPoint - sample.position);
                if (distance < closestDistanceSqr)
                {
                    closestDistanceSqr = distance;
                    closestSampleIndex = index;
                }
            }

            return (closestSampleIndex, closestDistanceSqr);
        }
                
        public static (SplineSample, float) GetClosestSampleOnSpline(this SampleCollection sampleCollection, Vector3 fromPoint)
        {
            float closestDistanceSqr = Mathf.Infinity;
            SplineSample closestSample = default;
            foreach (SplineSample sample in sampleCollection.samples)
            {
                float distance = Vector3.SqrMagnitude(fromPoint - sample.position);
                if (distance < closestDistanceSqr)
                {
                    closestDistanceSqr = distance;
                    closestSample = sample;
                }
            }
            return (closestSample, closestDistanceSqr);
        }
    }
}
