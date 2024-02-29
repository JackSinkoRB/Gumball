using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public class RacerCar : AICar
    {
        
        //a gamesession can have a list of racers
        //on gamesession start, spawn the racers at the defined positions

        //a racer is not restricted to 1 lane like traffic car - it's desired position is 
        // - get the width around the car that the car can move
        
        //desired position is the middle (spline)
        //spline sample ahead (10-20)


        protected override (Chunk, Vector3, Quaternion)? GetPositionAhead(float distance)
        {
            return null;
        }
    }
}
