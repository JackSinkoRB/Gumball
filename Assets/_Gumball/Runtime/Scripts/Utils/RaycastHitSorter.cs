using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gumball
{
    public static class RaycastHitSorter
    {

        public static void SortRaycastHitsByDistance(RaycastHit[] hits)
        {
            int validHitsCount = 0;

            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit hit = hits[i];

                //check if the hit is valid (not null and has a positive distance)
                if (hit.collider != null && hit.distance > 0)
                {
                    hits[validHitsCount] = hit;
                    validHitsCount++;
                }
            }

            Array.Sort(hits, 0, validHitsCount, RaycastHitDistanceComparer.Default);
        }
        
        private class RaycastHitDistanceComparer : IComparer<RaycastHit>
        {
            private static RaycastHitDistanceComparer _default;
            public static RaycastHitDistanceComparer Default
            {
                get
                {
                    if (_default == null)
                        _default = new RaycastHitDistanceComparer();
                    return _default;
                }
            }

            public int Compare(RaycastHit x, RaycastHit y)
            {
                return x.distance.CompareTo(y.distance);
            }
        }
        
    }
}
