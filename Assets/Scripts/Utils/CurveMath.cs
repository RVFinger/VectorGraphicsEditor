using UnityEngine;

public struct CurveMath
{
    public static Vector2 CalculateBezierPoint(float t, Vector2 firstPoint, Vector2 firstHandle, Vector2 secondHandle, Vector2 secondPoint)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 p = uuu * firstPoint; //first term
        p += 3 * uu * t * firstHandle; //second term
        p += 3 * u * tt * secondHandle; //third term
        p += ttt * secondPoint; //fourth term
        return p;
    }

    public static Vector2 GetPointOnLine(Vector2 point, Vector2 otherPoint, float distanceQuotientToPoint)
    {
        Vector2 pointToOtherPoint = otherPoint - point;
        return point + pointToOtherPoint * distanceQuotientToPoint;
    }

    public static Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2, out bool found)
    {
        float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

        if (tmp == 0)
        {
            // No solution!
            found = false;
            return Vector2.zero;
        }

        float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

        found = true;

        return new Vector2(
            B1.x + (B2.x - B1.x) * mu,
            B1.y + (B2.y - B1.y) * mu
        );
    }

    public static Vector2 GetIntersectionPointCoordinates(Vector2 A, Vector2 ADirection, Vector2 B, Vector2 BDirection)
    {
        Vector2 B2 = B + BDirection;
        Vector2 A2 = A + ADirection;
        float tmp = (B2.x - B.x) * (A2.y - A.y) - (B2.y - B.y) * (A2.x - A.x);

        if (tmp == 0)
        {
            return Vector2.zero;
        }

        float mu = ((A.x - B.x) * (A2.y - A.y) - (A.y - B.y) * (A2.x - A.x)) / tmp;


        return new Vector2(
            B.x + (B2.x - B.x) * mu,
            B.y + (B2.y - B.y) * mu
        );
    }

    public static bool IsPointOnLine(Vector2 linePointA, Vector2 linePointB, Vector2 point)
    {
        float dxc = point.x - linePointA.x;
        float dyc = point.y - linePointA.y;

        float dxl = linePointB.x - linePointA.x;
        float dyl = linePointB.y - linePointA.y;

        float cross = dxc * dyl - dyc * dxl;
        return cross == 0;
    }

    public static Vector2 Rotate(Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }
}
