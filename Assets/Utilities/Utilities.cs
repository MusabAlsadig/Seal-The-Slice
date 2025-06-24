using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static PolygonDirection GetPolygonDirection(List<Vector2> vectors)
    {
        float area = CalculateArea(vectors);

        if (area < 0)
            return PolygonDirection.Clockwise;
        else if (area > 0)
            return PolygonDirection.CounterClockwise;
        else
            return PolygonDirection.Coliner;
    }

    public static PolygonDirection GetPolygonDirection(List<Point> Points)
    {
        float area = CalculateArea(Points);

        if (area < 0)
            return PolygonDirection.Clockwise;
        else if (area > 0)
            return PolygonDirection.CounterClockwise;
        else
            return PolygonDirection.Coliner;
    }

    private static float CalculateArea(List<Vector2> vectors)
    {
        float area = 0;
        for (int i = 0; i < vectors.Count; i++)
        {
            int nextIndex = i + 1 < vectors.Count ? i + 1 : 0;
            Vector2 point = vectors[i];
            Vector2 nextPoint = vectors[nextIndex];
            area += point.x * nextPoint.y - nextPoint.x * point.y;
        }

        area /= 2;

        return area;
    }
    
    private static float CalculateArea(List<Point> Points)
    {
        float area = 0;
        for (int i = 0; i < Points.Count; i++)
        {
            int nextIndex = i + 1 < Points.Count ? i + 1 : 0;
            Vector3 point = Points[i].Position;
            Vector3 nextPoint = Points[nextIndex].Position;
            area += point.x * nextPoint.y - nextPoint.x * point.y;
        }

        area /= 2;

        return area;
    }


    public static Vector2 GetCenter(List<Vector2> vectors)
    {
        Vector2 center = Vector2.zero;

        vectors.ForEach(v => center += v);

        center /= vectors.Count;
        return center;
    }

    public static Vector2 Rotate(Vector2 vectorToRotate, Vector2 centerPoint, float angleInDegrees, PolygonDirection direction)
    {
        if (direction == PolygonDirection.Clockwise)
            angleInDegrees *= -1;

        float angleInRadians = angleInDegrees * (Mathf.PI / 180);
        float cosTheta = Mathf.Cos(angleInRadians);
        float sinTheta = Mathf.Sin(angleInRadians);
        return new Vector2
        {
            x =
                cosTheta * (vectorToRotate.x - centerPoint.x) -
                sinTheta * (vectorToRotate.y - centerPoint.y) + centerPoint.x,
            y =
                sinTheta * (vectorToRotate.x - centerPoint.x) +
                cosTheta * (vectorToRotate.y - centerPoint.y) + centerPoint.y
        };
    }

    public enum PolygonDirection
    {
        Clockwise = -1,
        Coliner = 0,
        CounterClockwise = 1,
    }
}


