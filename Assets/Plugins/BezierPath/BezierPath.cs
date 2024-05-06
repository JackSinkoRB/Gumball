using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierPathPoint
{
    public Vector3 position;
    public float distance;
    public float ratio;
#if UNITY_EDITOR
    public float bezierRatio; // SHOULD ONLY BE USED IN THE EDITOR!!
#endif

    public BezierPathPoint Copy(BezierPathPoint output = null)
    {
        if (output == null)
            output = new BezierPathPoint();

        output.position = position;
        output.distance = distance;
        output.ratio = ratio;
#if UNITY_EDITOR
        output.bezierRatio = bezierRatio;
#endif
        return output;
    }

    public static BezierPathPoint Lerp(BezierPathPoint a, BezierPathPoint b, float t, BezierPathPoint output = null)
    {
        if (output == null)
            output = new BezierPathPoint();

        output.position = Vector3.LerpUnclamped(a.position, b.position, t);
        output.distance = Mathf.LerpUnclamped(a.distance, b.distance, t);
        output.ratio = Mathf.LerpUnclamped(a.ratio, b.ratio, t);
#if UNITY_EDITOR
        output.bezierRatio = Mathf.LerpUnclamped(a.bezierRatio, b.bezierRatio, t);
#endif
        return output;
    }
}

public static class BezierPathTools
{
    // Calculates 2 control points that will create a smooth bezier curve.
    // http://www.antigrain.com/research/bezier_interpolation/
    static void SmoothBezierControlPoints(Vector3 a, Vector3 b, Vector3 c, out Vector3 outab, out Vector3 outbc, float curvature)
    {
        Vector3 ab = (a + b) * 0.5f;
        Vector3 bc = (c + b) * 0.5f;
        float abmagnitude = (a - b).magnitude;
        float bcmagnitude = (b - c).magnitude;
        Vector3 midpoint = Vector3.Lerp(ab, bc, abmagnitude / (abmagnitude + bcmagnitude));
        outab = b + (ab - midpoint) * curvature;
        outbc = b + (bc - midpoint) * curvature;
    }
    
    // Clamps an index to 0 to (total - 1), where overflow will circle around to the start/end.
    static int CircularClamp(int v, int total)
    {
        if (v >= total)
            return v % total;
        else if (v < 0)
            return total + (v % total);
        return v;
    }

    public static Vector3[] GenerateSmoothControlPoints(List<Vector3> bezierpoints, float curvature, bool loop)
    {
        int segmentcount = loop ? bezierpoints.Count : bezierpoints.Count - 1;
        Vector3[] cp = new Vector3[segmentcount * 2];

        // treat the first and last points differently
        int start = 0;
        int end = bezierpoints.Count;
        if (!loop)
        {
            start = 1;
            end = bezierpoints.Count - 1;
        }

        // do the middle points
        for (int i = start; i < end; ++i)
        {
            Vector3 a = bezierpoints[CircularClamp(i - 1, bezierpoints.Count)];
            Vector3 b = bezierpoints[i];
            Vector3 c = bezierpoints[(i + 1) % bezierpoints.Count];
            SmoothBezierControlPoints(a, b, c, out cp[(i - start) * 2 + start], out cp[(i - start) * 2 + start + 1], curvature);
        }

        // treat the first and last points differently
        if (!loop)
        {
            cp[0] = bezierpoints[0] + (cp[1] - bezierpoints[0]).normalized * Vector3.Distance(cp[1], bezierpoints[1]);
            cp[cp.Length - 1] = bezierpoints[bezierpoints.Count - 1] + (cp[cp.Length - 2] - bezierpoints[bezierpoints.Count - 1]).normalized * Vector3.Distance(cp[cp.Length - 2], bezierpoints[bezierpoints.Count - 2]);
        }

        return cp;
    }

    // Returns a point on a bezier curve where b is a control point
    public static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return Vector3.LerpUnclamped(Vector3.LerpUnclamped(a, b, t), Vector3.LerpUnclamped(b, c, t), t);
    }

    // Returns a point on a bezier curve where b and c are control points
    public static Vector3 Bezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 ab = Vector3.LerpUnclamped(a, b, t);
        Vector3 bc = Vector3.LerpUnclamped(b, c, t);
        Vector3 cd = Vector3.LerpUnclamped(c, d, t);
        return Bezier(ab, bc, cd, t);
    }

    static float PointLineDistance(Vector3 point, Vector3 start, Vector3 end)
    {
        if (start == end)
            return Vector3.Distance(point, start);

        Vector3 dir = (end - start).normalized;
        float d = Vector3.Dot(point - start, dir);
        return Vector3.Distance(point, start + dir * d);
    }

    // Simplifies points in a polyline
    static List<BezierPathPoint> DouglasPeucker(List<BezierPathPoint> points, int startIndex, int lastIndex, float epsilon)
    {
        float dmax = 0f;
        int index = startIndex;

        for (int i = index + 1; i < lastIndex; ++i)
        {
            float d = PointLineDistance(points[i].position, points[startIndex].position, points[lastIndex].position);
            if (d > dmax)
            {
                index = i;
                dmax = d;
            }
        }

        if (dmax > epsilon)
        {
            var res1 = DouglasPeucker(points, startIndex, index, epsilon);
            var res2 = DouglasPeucker(points, index, lastIndex, epsilon);

            var finalRes = new List<BezierPathPoint>();
            for (int i = 0; i < res1.Count - 1; ++i)
                finalRes.Add(res1[i]);

            for (int i = 0; i < res2.Count; ++i)
                finalRes.Add(res2[i]);

            return finalRes;
        }

        return new List<BezierPathPoint>(new BezierPathPoint[] { points[startIndex], points[lastIndex] });
    }

    public static List<BezierPathPoint> OptimiseLinearPoints(List<BezierPathPoint> points, float optimiserange)
    {
        return DouglasPeucker(points, 0, points.Count - 1, optimiserange);
    }

    public static float GetClosestRatioPointOnLineSegment(Vector3 A, Vector3 B, Vector3 P)
    {
        Vector3 AP = P - A;
        Vector3 AB = B - A;
        float magnitudeAB = AB.sqrMagnitude;
        float ABAPproduct = Vector3.Dot(AP, AB);
        return Mathf.Clamp01(ABAPproduct / magnitudeAB);
    }
}

/*
    Bezier Jargon:
        Position: World vector position of a point on the bezier curve
        BezierT: The distance along the path where the start is 0 and end is bezierPoints.Count
        T: The distance along the path where the start is 0 and the end is cachedPoints.Count
        Ratio: The distance along the path where the start is 0 and the end is 1
        Distance: The distance along the where the start is 0 and the end is the approximated world distance to the end
*/
public class BezierPath : MonoBehaviour
{
    [HideInInspector]
    public List<BezierPathPoint> cachedPoints = new List<BezierPathPoint>();
    [HideInInspector]
    public Bounds bounds;
    public float totalDistance { get { return cachedPoints.Count > 0 ? cachedPoints[cachedPoints.Count - 1].distance : 0; } }
    public BezierPathPoint finalPoint { get { return cachedPoints[cachedPoints.Count - 1]; } }

    [Header("Generation")]
    public List<Vector3> bezierPoints = new List<Vector3>();
    public bool loop = false;
    public float curvature = 1;
    public int pointsPerSegment = 8;
    [Range(0.0f, 5.0f)]
    public float optimiseRange = 0;

    [Header("Display")]
    public Color displayLineColor = Color.red;
    public Color displayHandleColor = Color.blue;
    public float displayPointSize = 0.2f;

    //////////////// BUILDING METHODS ////////////////

    public void Rebuild()
    {
        if (bezierPoints.Count < 2)
        {
            cachedPoints = null;
            return;
        }
        else if (bezierPoints.Count == 2)
        {
            cachedPoints = new List<BezierPathPoint>();
            
            BezierPathPoint first = new BezierPathPoint();
            first.position = bezierPoints[0];
            first.distance = 0;
            first.ratio = 0;
#if UNITY_EDITOR
            first.bezierRatio = 0;
#endif
            cachedPoints.Add(first);

            BezierPathPoint second = new BezierPathPoint();
            second.position = bezierPoints[1];
            second.distance = Vector3.Distance(first.position, second.position);
            second.ratio = 1;
#if UNITY_EDITOR
            second.bezierRatio = 1;
#endif
            cachedPoints.Add(second);

            RecalculateBounds();
            return;
        }

        Vector3[] cps = BezierPathTools.GenerateSmoothControlPoints(bezierPoints, curvature, loop);

        int cpoffset = loop ? 1 : 0;
        int segmentcount = loop ? bezierPoints.Count : bezierPoints.Count - 1;
        int cachepointcount = segmentcount * pointsPerSegment + (loop ? 0 : 1);
        if (cachedPoints == null)
            cachedPoints = new List<BezierPathPoint>(cachepointcount);
        else
            cachedPoints.Clear();

        for (int i = 0; i < segmentcount; ++i)
        {
            Vector3 p0 = bezierPoints[i];
            Vector3 cp0 = cps[i * 2 + cpoffset];
            Vector3 cp1 = cps[(i * 2 + 1 + cpoffset) % cps.Length];
            Vector3 p1 = bezierPoints[(i + 1) % bezierPoints.Count];

            for (int s = 0; s < pointsPerSegment; ++s)
            {
                float t = (float)s / pointsPerSegment;

                BezierPathPoint point = new BezierPathPoint();
                point.position = BezierPathTools.Bezier(p0, cp0, cp1, p1, t);
#if UNITY_EDITOR
                point.bezierRatio = i + t;
#endif
                cachedPoints.Add(point);
            }
        }
        if (!loop)
            cachedPoints[cachedPoints.Count - 1].position = bezierPoints[bezierPoints.Count - 1];

        // optimise points
        cachedPoints = BezierPathTools.OptimiseLinearPoints(cachedPoints, optimiseRange);

        // calcaulte lengths
        cachedPoints[0].distance = 0;
        for (int i = 1; i < cachedPoints.Count; ++i)
            cachedPoints[i].distance = cachedPoints[i - 1].distance + Vector3.Distance(cachedPoints[i - 1].position, cachedPoints[i].position);

        // calcaulte ratios
        for (int i = 0; i < cachedPoints.Count; ++i)
            cachedPoints[i].ratio = cachedPoints[i].distance / totalDistance;

        // recalculate bounds
        RecalculateBounds();
    }

    void RecalculateBounds()
    {
        bounds = new Bounds(cachedPoints[0].position, Vector3.zero);
        for (int i = 1; i < cachedPoints.Count; ++i)
            bounds.Encapsulate(cachedPoints[i].position);
    }

    //////////////// GET T METHODS ////////////////
    
    ///<summary>Finds T that contains the point at distance (TODO: optimise by using a better search algorithm)</summary>
    public float GetTAtDistance(float distance)
    {
        if (distance <= 0)
            return 0;
        else if (distance >= totalDistance)
            return cachedPoints.Count - 1;

        for (int i = 1; i < cachedPoints.Count; ++i)
        {
            if (cachedPoints[i].distance > distance)
                return (i - 1) + Mathf.InverseLerp(cachedPoints[i - 1].distance, cachedPoints[i].distance, distance);
        }
        return cachedPoints.Count - 1;
    }

    // Finds T that contains the point at ratio (TODO: optimise by using a better search algorithm)
    public float GetTAtRatio(float ratio)
    {
        if (ratio <= 0)
            return 0;
        else if (ratio >= 1)
            return cachedPoints.Count - 1;

        for (int i = 1; i < cachedPoints.Count; ++i)
        {
            if (cachedPoints[i].ratio > ratio)
                return (i - 1) + Mathf.InverseLerp(cachedPoints[i - 1].ratio, cachedPoints[i].ratio, ratio);
        }
        return cachedPoints.Count - 1;
    }

#if UNITY_EDITOR
    // Finds T that contains the point at bezierratio (TODO: optimise by using a better search algorithm)
    public float GetTAtBezierRatio(float bezierratio)
    {
        if (bezierratio <= 0)
            return 0;
        else if (bezierratio >= 1)
            return bezierPoints.Count - 1;

        for (int i = 1; i < cachedPoints.Count; ++i)
        {
            if (cachedPoints[i].bezierRatio > bezierratio)
                return (i - 1) + Mathf.InverseLerp(cachedPoints[i - 1].bezierRatio, cachedPoints[i].bezierRatio, bezierratio);
        }
        return cachedPoints.Count - 1;
    }
#endif

    // Finds the nearest T to the specified world position
    public float GetNearestTAtPosition(Vector3 p, float refdist = float.NaN, float furthestdistance = float.NaN)
    {
        bool isnearestwithindistance = false;
        float nearestT = 0;
        float nearestdistsqr = float.MaxValue;
        for (int i = 0; i < cachedPoints.Count - 1; ++i)
        {
            float subratio = BezierPathTools.GetClosestRatioPointOnLineSegment(cachedPoints[i].position, cachedPoints[i + 1].position, p);
            Vector3 point = Vector3.LerpUnclamped(cachedPoints[i].position, cachedPoints[i + 1].position, subratio);

            bool iswithindistance = true;
            if (furthestdistance != float.NaN)
            {
                float dist = Mathf.LerpUnclamped(cachedPoints[i].distance, cachedPoints[i + 1].distance, subratio);
                dist = Mathf.Abs(refdist - dist);
                if (loop && dist > totalDistance * 0.5f)
                    dist = totalDistance - dist;

                iswithindistance = dist < furthestdistance;
                if (!iswithindistance && isnearestwithindistance)
                    continue;
            }

            float distsqr = (p - point).sqrMagnitude;
            if (iswithindistance == isnearestwithindistance && distsqr > nearestdistsqr)
                continue;

            isnearestwithindistance = iswithindistance;
            nearestT = i + subratio;
            nearestdistsqr = distsqr;
        }
        return nearestT;
    }

    //////////////// GET POINT METHODS ////////////////

    // Finds BezierPathPoint that contains the point at distance (TODO: optimise by using a better search algorithm)
    public BezierPathPoint GetPointAtDistance(float distance, BezierPathPoint output = null)
    {
        return GetPointFromT(GetTAtDistance(distance), output);
    }

    // Finds BezierPathPoint that contains the point at ratio (TODO: optimise by using a better search algorithm)
    public BezierPathPoint GetPointAtRatio(float ratio, BezierPathPoint output = null)
    {
        return GetPointFromT(GetTAtRatio(ratio), output);
    }

#if UNITY_EDITOR
    // Finds BezierPathPoint that contains the point at bezierratio (TODO: optimise by using a better search algorithm)
    public BezierPathPoint GetPointAtBezierRatio(float bezierratio, BezierPathPoint output = null)
    {
        return GetPointFromT(GetTAtBezierRatio(bezierratio), output);
    }
#endif

    // Finds the nearest T to the specified world position
    public BezierPathPoint GetNearestPointAtPosition(Vector3 p, BezierPathPoint output = null)
    {
        return GetPointFromT(GetNearestTAtPosition(p), output);
    }

    // Finds the nearest T to the specified world position
    public BezierPathPoint GetNearestPointAtPosition(Vector3 p, float refdist, float furthestdistance, BezierPathPoint output = null)
    {
        return GetPointFromT(GetNearestTAtPosition(p, refdist, furthestdistance), output);
    }

    //////////////// HELPER METHODS ////////////////

    // Finds the lerped SmoothBezierPoint that contains the point at distance
    public BezierPathPoint GetPointFromT(float T, BezierPathPoint output = null)
    {
        if (cachedPoints.Count == 0)
        {
            Debug.LogError("The bezier path does not have any points!", this);
            return null;
        }

        int idx = Mathf.FloorToInt(T);
        if (idx < 0)
            return cachedPoints[0].Copy(output);
        else if (idx >= cachedPoints.Count - 1)
            return cachedPoints[cachedPoints.Count - 1].Copy(output);

        return BezierPathPoint.Lerp(cachedPoints[idx], cachedPoints[idx + 1], T - idx, output);
    }

    public float GetRatioAtDistance(float distance)
    {
        return distance / totalDistance;
    }

    public float GetRatioFromT(float t)
    {
        return t / (cachedPoints.Count - 1);
    }

    public float GetTFromRatio(float ratio)
    {
        return ratio * (cachedPoints.Count - 1);
    }

    public float GetDistanceAtT(float t)
    {
        return GetPointFromT(t).distance;
    }
}
