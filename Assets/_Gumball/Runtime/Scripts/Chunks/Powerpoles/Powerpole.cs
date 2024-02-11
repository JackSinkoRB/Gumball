using System.Collections;
using System.Collections.Generic;
using Dreamteck.Splines;
using MyBox;
using UnityEngine;

namespace Gumball
{
    public class Powerpole : MonoBehaviour
    {

        
        public enum PowerpolePosition
        {
            NONE,
            LEFT,
            RIGHT
        }
        
        [SerializeField] private PowerpoleLinePosition[] linePositions;

        [SerializeField, ReadOnly] private PowerpolePosition position;
        [SerializeField, ReadOnly] private int closestSplineIndex;
        [SerializeField, ReadOnly] private float crossProductToSpline;
        
        public PowerpolePosition Position => position;
        public int ClosestSplineIndex => closestSplineIndex;
        
        public void CalculatePosition(Chunk chunk)
        {
            var (closestSampleIndex, distanceToSplineSqr) = chunk.GetClosestSampleIndexOnSpline(transform.position);

            closestSplineIndex = closestSampleIndex;
            SplineSample sample = chunk.SplineSamples[closestSampleIndex];

            Vector2 a = sample.position.FlattenAsVector2();
            Vector2 b = (sample.position + sample.forward).FlattenAsVector2();
            Vector2 c = transform.position.FlattenAsVector2();
            crossProductToSpline = (b.x - a.x)*(c.y - a.y) - (b.y - a.y)*(c.x - a.x);
            
            position = crossProductToSpline > 0 ? PowerpolePosition.LEFT : PowerpolePosition.RIGHT;
        }

        public void ConnectLines(Powerpole otherPole)
        {
            for (int index = 0; index < linePositions.Length; index++)
            {
                PowerpoleLinePosition linePosition = linePositions[index];
                PowerpoleLinePosition otherLinePosition = otherPole.linePositions[index];
                
                linePosition.CreateLine(otherLinePosition);
            }
        }
        

        
    }
}
