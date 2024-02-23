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
        
    }
}
